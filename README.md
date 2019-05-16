
* Experimental presentation based user interface for PowerShell
* WPF user interface
* Pure C#. No XAML.
* Minimalist. Currently only [200 lines long](https://github.com/dharmatech/PsReplWpf/blob/master/PsReplWpfTextBlock/MainWindow.cs).
* I encourage folks to fork and experiment with new approaches. Sharing of your results is welcome!

# Screenshots

Results of an `ls` are shown in a `DataGrid`. Actions are available via a context menu:

![](https://i.imgur.com/ddoUoet.gif)

Similar for services:

![](https://i.imgur.com/9o23JQO.gif)

And processes:

![](https://i.imgur.com/n9qugOL.gif)

JSON retrieved via `Invoke-RestMethod` is displayed in a `TreeView` if `$output_to_tree_view` is set to `$true`:

![](https://i.imgur.com/4slZGbg.gif)

You can of course always output to `Out-String` to display data in the traditional PowerShell format:

![](https://i.imgur.com/YL6g9X0.png)

Let's to go a folder containing images and list the files there:

![](https://i.imgur.com/l7KWzz2.png)

# Status

* This is currently a pure experimental proof-of-concept stage project.
* The implementation is not PSHost based (and thus has many limitatons).

# See Also

* [Symbolics Lisp Machine Listener](https://youtu.be/o4-YnLpLgtk)
* [Presentation Based User Interfaces](https://dspace.mit.edu/handle/1721.1/41161)
