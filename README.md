# PsReplWpf

* Experimental presentation based user interface for PowerShell
* WPF user interface
* Pure C#. No XAML.
* Minimalist. Currently only 200 lines long.
* I encourage folks to fork and experiment with new approaches. Sharing of your results is welcome!

# Screenshots

* Folders show up as buttons.
    - Clicking the button changes the current directory to that directory in the shell.
    - The context menu for a folder has an entry to open the folder in Explorer:

![](https://i.imgur.com/gGrLvhL.png)

* Get-Process - processes display as buttons
    - Process context menu has a 'Stop Process' item

![](https://i.imgur.com/Eed010C.png)

You can of course always output to `Out-String` to display data in the traditional PowerShell format:

![](https://i.imgur.com/YL6g9X0.png)

Services have a context menu with items for stopping, starting, and pausing:

![](https://i.imgur.com/4Yr6E9l.png)

# Status

* This is currently a pure experimental proof-of-concept stage project.
* The implementation is not PSHost based (and thus has many limitatons).

# See Also

* [Symbolics Lisp Machine Listener](https://youtu.be/o4-YnLpLgtk)
* [Presentation Based User Interfaces](https://dspace.mit.edu/handle/1721.1/41161)
