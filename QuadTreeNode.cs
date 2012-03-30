using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;

namespace SimpleQuadTree
{
    /// <summary>
    /// The QuadTreeNode
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QuadTreeNode<T>
    {
        /// <summary>
        /// Construct a quadtree node with the given bounds 
        /// </summary>
        /// <param name="area"></param>
        public QuadTreeNode(RectangleF bounds, QuadTree<T> tree)
        {
			Tree = tree;
            m_bounds = bounds;
        }
		
		public QuadTree<T> Tree {get; set;}

        /// <summary>
        /// The area of this node
        /// </summary>
        RectangleF m_bounds;

        /// <summary>
        /// The contents of this node.
        /// Note that the contents have no limit: this is not the standard way to impement a QuadTree
        /// </summary>
        List<T> m_contents = new List<T>();

        /// <summary>
        /// The child nodes of the QuadTree
        /// </summary>
        List<QuadTreeNode<T>> m_nodes = new List<QuadTreeNode<T>>(4);

        /// <summary>
        /// Is the node empty
        /// </summary>
        public bool IsEmpty { get { return this.Contents.Count == 0 && (m_bounds.IsEmpty || m_nodes.Count == 0); } }

        /// <summary>
        /// Area of the quadtree node
        /// </summary>
        public RectangleF Bounds { get { return m_bounds; } }

        /// <summary>
        /// Total number of nodes in the this node and all SubNodes
        /// </summary>
        public int Count
        {
            get
            {
                int count = 0;

                foreach (QuadTreeNode<T> node in m_nodes)
                    count += node.Count;

                count += this.Contents.Count;

                return count;
            }
        }

        /// <summary>
        /// Return the contents of this node and all subnodes in the true below this one.
        /// </summary>
        public IEnumerable<T> SubTreeContents
        {
            get
            {
                foreach (QuadTreeNode<T> node in m_nodes)
					foreach (T t in node.SubTreeContents)
						yield return t;

				foreach (T t in this.Contents)
					yield return t;
            }
        }

        public List<T> Contents { get { return m_contents; } }

        /// <summary>
        /// Query the QuadTree for items that are in the given area
        /// </summary>
        /// <param name="queryArea"></pasram>
        /// <returns></returns>
        public IEnumerable<T> Query(RectangleF queryArea)
        {
            // this quad contains items that are not entirely contained by
            // it's four sub-quads. Iterate through the items in this quad 
            // to see if they intersect.
            foreach (T item in this.Contents)
            {
                if (queryArea.IntersectsWith(Tree.GetRect(item)))
                    yield return item;
            }

            foreach (QuadTreeNode<T> node in m_nodes)
            {
                if (node.IsEmpty)
                    continue;

                // Case 1: search area completely contained by sub-quad
                // if a node completely contains the query area, go down that branch
                // and skip the remaining nodes (break this loop)
                if (node.Bounds.Contains(queryArea))
                {
					foreach (T t in node.Query (queryArea))
						yield return t;
                    break;
                }

                // Case 2: Sub-quad completely contained by search area 
                // if the query area completely contains a sub-quad,
                // just add all the contents of that quad and it's children 
                // to the result set. You need to continue the loop to test 
                // the other quads
                if (queryArea.Contains(node.Bounds))
                {
					foreach (T t in node.SubTreeContents)
						yield return t;
                    continue;
                }

                // Case 3: search area intersects with sub-quad
                // traverse into this quad, continue the loop to search other
                // quads
                if (node.Bounds.IntersectsWith(queryArea))
                {
					foreach (T t in node.Query (queryArea))
						yield return t;
				}
            }
        }

        /// <summary>
        /// Insert an item to this node
        /// </summary>
        /// <param name="item"></param>
        public void Insert(T item)
        {
            // if the item is not contained in this quad, there's a problem
            if (!m_bounds.Contains(Tree.GetRect(item)))
            {
                Trace.TraceWarning("feature is out of the bounds of this quadtree node");
                return;
            }

            // if the subnodes are null create them. may not be sucessfull: see below
            // we may be at the smallest allowed size in which case the subnodes will not be created
            //if (m_nodes.Count == 0)
            //    CreateSubNodes();
			
			if (m_nodes.Count == 0 && this.Contents.Count >= Tree.NodeCapacity)
			{
				CreateSubNodes ();
				MoveContentsToSubNodes ();				
			}
			
			if (this.Contents.Count > Tree.NodeCapacity)
			{
				//this node is full, let's try and store T in a subnode, if it's small enough.
				
	            // for each subnode:
	            // if the node contains the item, add the item to that node and return
	            // this recurses into the node that is just large enough to fit this item
	            foreach (QuadTreeNode<T> node in m_nodes)
	            {
	                if (node.Bounds.Contains(Tree.GetRect(item)))
	                {
	                    node.Insert(item);
	                    return;
	                }
	            }
			}
			//add, even if we are over capacity.
			this.Contents.Add (item);
        }

		void MoveContentsToSubNodes ()
		{
			Contents.RemoveAll (t => {
        		foreach (var n in m_nodes)
        		{
        			if (n.Bounds.Contains (Tree.GetRect(t)))
        			{
        				n.Insert (t);
        				return true;
        			}
        		}
				return false;
        	});
		}

        public void ForEach(QuadTree<T>.QTAction action)
        {
            action(this);

            // draw the child quads
            foreach (QuadTreeNode<T> node in this.m_nodes)
                node.ForEach(action);
        }
		
		public IEnumerable<QuadTreeNode<T>> Nodes
		{
			get {
				foreach (var node in this.m_nodes)
				{
					yield return node;
					foreach (var sub in node.m_nodes)
						yield return sub;
				}
			}
		}

        /// <summary>
        /// Internal method to create the subnodes (partitions space)
        /// </summary>
        private void CreateSubNodes()
        {
            // the smallest subnode has an area 
            if ((m_bounds.Height * m_bounds.Width) <= Tree.MinNodeSize)
                return;

            float halfWidth = (m_bounds.Width / 2f);
            float halfHeight = (m_bounds.Height / 2f);

			m_nodes.Add(new QuadTreeNode<T>(new RectangleF(m_bounds.Location, new SizeF(halfWidth, halfHeight)), Tree));
            m_nodes.Add(new QuadTreeNode<T>(new RectangleF(new PointF(m_bounds.Left, m_bounds.Top + halfHeight), new SizeF(halfWidth, halfHeight)), Tree));
            m_nodes.Add(new QuadTreeNode<T>(new RectangleF(new PointF(m_bounds.Left + halfWidth, m_bounds.Top), new SizeF(halfWidth, halfHeight)), Tree));
            m_nodes.Add(new QuadTreeNode<T>(new RectangleF(new PointF(m_bounds.Left + halfWidth, m_bounds.Top + halfHeight), new SizeF(halfWidth, halfHeight)),Tree));
        }

    }
}
