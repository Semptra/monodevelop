// LocalsPad.cs
//
// Author:
//   Lluis Sanchez Gual <lluis@novell.com>
//
// Copyright (c) 2008 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//

using System.Linq;

using Mono.Debugging.Client;

namespace MonoDevelop.Debugger
{
	public class LocalsPad : ObjectValuePad
	{
		public LocalsPad ()
		{
			if (UseNewTreeView) {
				controller.AllowEditing = true;
			} else {
				tree.AllowEditing = true;
				tree.AllowAdding = false;
			}
		}

		void ReloadValues ()
		{
			var frame = DebuggingService.CurrentFrame;

			if (frame == null)
				return;

			var locals = frame.GetAllLocals ();

			DebuggerLoggingService.LogMessage ("Begin Local Variables:");
			foreach (var local in locals)
				DebuggerLoggingService.LogMessage ("\t{0}", local.Name);
			DebuggerLoggingService.LogMessage ("End Local Variables");

			if (UseNewTreeView) {
				controller.ClearValues ();
				controller.AddValues (locals);

				//var xx = new System.Collections.Generic.List<ObjectValueNode> ();

				//xx.Add (new FakeObjectValueNode ("f1"));
				//xx.Add (new FakeIsImplicitNotSupportedObjectValueNode ());

				//xx.Add (new FakeEvaluatingGroupObjectValueNode (1));
				//xx.Add (new FakeEvaluatingGroupObjectValueNode (0));
				//xx.Add (new FakeEvaluatingGroupObjectValueNode (5));

				//xx.Add (new FakeEvaluatingObjectValueNode ());
				//xx.Add (new FakeEnumerableObjectValueNode (10));
				//xx.Add (new FakeEnumerableObjectValueNode (20));
				//xx.Add (new FakeEnumerableObjectValueNode (23));

				//controller.AddValues (xx);
			} else {
				tree.ClearValues ();
				tree.AddValues (locals);
			}
		}

		public override void OnUpdateFrame ()
		{
			base.OnUpdateFrame ();
			ReloadValues ();
		}

		public override void OnUpdateValues ()
		{
			base.OnUpdateValues ();
			ReloadValues ();
		}
	}
}
