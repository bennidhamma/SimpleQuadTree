using System;
using NUnit.Framework;
using SimpleQuadTree;
using System.Drawing;
using System.Linq;
using NUnit.Framework.SyntaxHelpers;

namespace Tests
{
	[TestFixture()]
	public class Tests
	{
		[Test()]
		public void TestSubdivision ()
		{
			Random rand = new Random ();
			var bounds = new RectangleF (0f,0f,100f,100f);
			QuadTree<RectangleF> tree = new QuadTree<RectangleF>(bounds) {
				GetRect = r => r
			};
			
			//add 10 items, assert no subnodes.
			for (int i = 0; i < 10; i++)
			{
				var rect = new RectangleF ( rand.Next (0, 80), rand.Next (0, 80), rand.Next (1, 10), rand.Next (1, 10));
				tree.Insert (rect);
			}
			
			Assert.That (tree.Nodes.Count (), Is.EqualTo (1));
			
			//add 11th item, assert there are subnodes.
			
			var lastRect = new RectangleF (rand.Next (0, 80), rand.Next (0, 80), rand.Next (1, 10), rand.Next (1, 10));
			tree.Insert (lastRect);
			
			Assert.That (tree.Nodes.Count (), Is.EqualTo (5));
		}
		
		[Test()]
		public void Test1000ItemsWithQuery ()
		{
			Random rand = new Random ();
			var bounds = new RectangleF (0f,0f,100f,100f);
			QuadTree<RectangleF> tree = new QuadTree<RectangleF>(bounds) {
				GetRect = r => r
			};
			
			//add 10 items, assert no subnodes.
			for (int i = 0; i < 1000; i++)
			{
				var rect = new RectangleF ( rand.Next (0, 80), rand.Next (0, 80), rand.Next (1, 10), rand.Next (1, 10));
				tree.Insert (rect);
			}
			
			//insert a rect at 25, 25, 5, 5, and query for it.
			
			var searchRect = new RectangleF (25f, 25f, 5f, 5f);
			tree.Insert (searchRect);
			
			//now try and find it.
			
			bool found = false;
			int count = 0;
			foreach (var rect in tree.Query (new RectangleF (20f, 20f, 10f, 10f)))
			{
				count++;
				Console.WriteLine (rect);
				if (rect == searchRect)
				{
					found = true;
					break;
				}
			}
			
			Console.WriteLine ("count: " + count);

			Assert.That (found, Is.True);
		}
	}
}

