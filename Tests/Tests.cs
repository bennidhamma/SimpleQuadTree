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
		public void TestCase ()
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
		}
	}
}

