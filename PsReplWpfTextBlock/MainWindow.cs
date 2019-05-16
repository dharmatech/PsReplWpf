using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dharmatech.WpfExtensionMethods;

using static System.Console;

namespace PsReplWpfTextBlock
{
    public class MainWindow : Window
    {
        static Brush foreground = new SolidColorBrush(Colors.WhiteSmoke);

        static Brush background = new SolidColorBrush(Color.FromRgb(1, 36, 86));


        static Style column_header_style = new Style(typeof(DataGridColumnHeader))
            .AddSetter(new Setter(DataGridColumnHeader.BackgroundProperty, background))
            .AddSetter(new Setter(DataGridColumnHeader.ForegroundProperty, foreground));

        static Style row_header_style = new Style(typeof(DataGridRowHeader))
            .AddSetter(new Setter(DataGridRowHeader.BackgroundProperty, background));

        static Style tree_view_item_style = new Style(typeof(TreeViewItem))
            .AddSetter(
                new EventSetter(
                    RequestBringIntoViewEvent,
                    (RequestBringIntoViewEventHandler)((s, e) => e.Handled = true)));

        static Style row_style = new Style(typeof(DataGridRow))
            .AddSetter(new Setter(DataGridRow.BackgroundProperty, background))
            .AddSetter(new Setter(DataGridRow.ForegroundProperty, foreground))
            .AddTrigger(
                new Trigger()
                {
                    Property = IsMouseOverProperty,
                    Value = true
                }
                .AddSetter(new Setter(BackgroundProperty, Brushes.Green))
            );

        
        static DataGridTextColumn make_data_grid_text_column(string header, int width) =>
            new DataGridTextColumn()
            {
                Header = header,
                Binding = new Binding(header),
                Width = width
            };

        static DataGridTextColumn make_data_grid_text_column(string header) =>
            new DataGridTextColumn()
            {
                Header = header,
                Binding = new Binding(header)
            };

        
        static void add_property(ItemsControl parent, PSPropertyInfo info)
        {
            try
            {
                var tree_view_item = new TreeViewItem()
                {
                    Header = String.Format("{0} : {1}", info.Name.ToString(), info.Value == null ? "" : info.Value.ToString()),
                    Foreground = foreground,
                    ItemContainerStyle = tree_view_item_style
                };

                if (info.Value is PSObject obj)
                {
                    // WriteLine("info.Value is a PSObject   info.Name: {0}   info.Value: {1}", info.Name, info.Value);

                    foreach (var elt in obj.Properties)
                        add_property(tree_view_item, elt);
                }
                else if (LanguagePrimitives.GetEnumerator(info.Value) != null)
                {
                    // WriteLine(String.Format("info.Name : {0}    info.Value is collection", info.Name));

                    foreach (var elt in LanguagePrimitives.GetEnumerable(info.Value))
                    {
                        // WriteLine("{0}   type: {1}", elt, elt.GetType());

                        if (elt is PSObject elt_obj)
                        {
                            var item = new TreeViewItem()
                            {
                                Header = elt_obj.ToString(),
                                Foreground = foreground,
                                ItemContainerStyle = tree_view_item_style
                            };

                            foreach (var elt_info in elt_obj.Properties)
                                add_property(item, elt_info);

                            tree_view_item.Items.Add(item);
                        }
                    }
                }

                else if (info.Value == null)
                {
                    // WriteLine(String.Format("info.Name : {0}   info.Value is null", info.Name));
                }

                else
                {
                    if (new PSObject(info.Value).Properties.Count() > 0)
                    {
                        tree_view_item.Items.Add("...");

                        tree_view_item.Expanded += (s, e) =>
                        {
                            tree_view_item.Items.Clear();

                            foreach (var elt in new PSObject(info.Value).Properties)
                                add_property(tree_view_item, elt);
                        };
                    }
                }

                parent.Items.Add(tree_view_item);
            }
            catch (Exception exception)
            {
                WriteLine("info.Name: {0}", info.Name);

                WriteLine(exception); WriteLine(); WriteLine();
            }
        }
               
        TreeView obj_to_tree_view(PSObject obj)
        {
            var tree_view = new TreeView()
            {
                MaxHeight = 400,
                ItemContainerStyle = tree_view_item_style
            };

            var tree_view_item = new TreeViewItem()
            {
                Header = String.Format("{0} : {1}", obj, obj.BaseObject.GetType()),
                Foreground = foreground,
                ItemContainerStyle = tree_view_item_style
            };

            foreach (var property_info in obj.Properties)
                add_property(tree_view_item, property_info);
            
            tree_view.Items.Add(tree_view_item);

            return tree_view;
        }

        TreeView obj_to_tree_view(System.Collections.ObjectModel.Collection<PSObject> objs)
        {
            var tree_view = new TreeView()
            {
                Background = background,
                MaxHeight = 400,
                ItemContainerStyle = tree_view_item_style
            };
                                    
            tree_view.Resources.Add(
                SystemColors.InactiveSelectionHighlightBrushKey,
                new SolidColorBrush(Color.FromRgb(44, 65, 95)));
            
            var tree_view_item = new TreeViewItem()
            {
                Header = String.Format("{0}", objs),
                IsExpanded = true,
                Foreground = foreground
            };

            foreach (var obj in objs)
            {
                var item = new TreeViewItem()
                {
                    Header = String.Format("{0} : {1}", obj, obj.BaseObject.GetType()),
                    Foreground = foreground,
                    ItemContainerStyle = tree_view_item_style
                };

                foreach (var info in obj.Properties)
                    add_property(item, info);

                tree_view_item.Items.Add(item);
            }

            tree_view.Items.Add(tree_view_item);

            return tree_view;
        }
               
        public MainWindow()
        {
            var dock_panel = new DockPanel();
            
            var scroll_viewer = new ScrollViewer();

            var text_block = new TextBlock()
            {
                FontFamily = new FontFamily("Lucida Console"),
                Background = new SolidColorBrush(Color.FromRgb(1, 36, 86)),
                Foreground = foreground
            };

            scroll_viewer.Content = text_block;
            
            var text_box = new TextBox()
            {
                FontFamily = new FontFamily("Lucida Console"),
                Background = new SolidColorBrush(Color.FromRgb(1, 36, 86)),
                Foreground = new SolidColorBrush(Colors.WhiteSmoke)
            }.SetDock(Dock.Bottom);

            var runspace = RunspaceFactory.CreateRunspace(InitialSessionState.CreateDefault());

            runspace.Open();

            void show_current_directory()
            {
                var ps = PowerShell.Create();

                ps.Runspace = runspace;

                var elts = ps.AddCommand("Get-Location").Invoke();

                if (elts.Count > 0)
                {
                    text_block.Inlines.Add(
                        new Label()
                        {
                            Content = elts.First().ToString(),
                            Foreground = new SolidColorBrush(Colors.Yellow)
                        });

                    text_block.Inlines.Add(new LineBreak());
                }

                scroll_viewer.ScrollToBottom();
            }

            text_block.ContextMenu = new ContextMenu().AddItems(
                new MenuItem() { Header = "Clear" }.AddClick((s, e) =>
                {
                    text_block.Inlines.Clear();

                    show_current_directory();
                }));
            
            show_current_directory();
            
            text_box.KeyDown += (sender, event_args) => 
            {
                if (event_args.Key == Key.Return)
                {
                    {
                        var text = text_box.Text;
                                            
                        text_block.Inlines.Add(
                            new Button()
                            {
                                Content = text_box.Text,
                                Foreground = new SolidColorBrush(Colors.Green),
                                Background = new SolidColorBrush(Color.FromRgb(1, 36, 86)),
                                // Foreground = new SolidColorBrush(Colors.WhiteSmoke),
                                Margin = new Thickness(5.0)
                            }.AddClick((s,e) => { text_box.Text = text; }));
                    }

                    // text_block.Inlines.Add(new Label() { Content = text_box.Text, Foreground = new SolidColorBrush(Colors.Green) });

                    text_block.Inlines.Add(new LineBreak());
                    
                    var ps = PowerShell.Create();

                    ps.Runspace = runspace;

                    var result = ps.AddScript(text_box.Text).Invoke();
                    
                    if (ps.HadErrors)
                    {
                        foreach (var elt in ps.Streams.Error)
                        {
                            text_block.Inlines.Add(new Label() { Content = elt.ToString(), Foreground = new SolidColorBrush(Colors.Red) });

                            text_block.Inlines.Add(new LineBreak());
                        }
                    }
                    else
                        try
                        {
                            var output_to_tree_view = false;

                            var output_thumbnails = false;

                            {
                                var ps_ = PowerShell.Create(); ps_.Runspace = runspace;

                                var result_ = ps_.AddScript("$output_to_tree_view").Invoke();

                                if (result_[0] != null && result_[0].BaseObject is bool && ((bool)result_[0].BaseObject))
                                {
                                    output_to_tree_view = true;
                                }
                            }

                            {
                                var ps_ = PowerShell.Create(); ps_.Runspace = runspace;

                                var result_ = ps_.AddScript("$output_thumbnails").Invoke();

                                if (result_[0] != null && result_[0].BaseObject is bool && ((bool)result_[0].BaseObject))
                                {
                                    output_thumbnails = true;
                                }
                            }

                            if (output_to_tree_view)
                            {
                                text_block.Inlines.Add(obj_to_tree_view(result));
                                text_block.Inlines.Add(new LineBreak());
                            }

                            else if (output_thumbnails)
                            {
                                foreach (var elt in result)
                                {
                                    if (elt.BaseObject is FileInfo file_info && (file_info.Extension == ".jpg" || file_info.Extension == ".png"))
                                    {
                                        text_block.Inlines.Add(
                                            new Image()
                                            {
                                                Source = new BitmapImage(new Uri(file_info.FullName)),
                                                MaxHeight = 100,
                                                MaxWidth = 100
                                            });

                                        text_block.Inlines.Add(file_info.Name);

                                        text_block.Inlines.Add(new LineBreak());
                                    }
                                    else
                                    {
                                        var str = elt.ToString();

                                        text_block.Inlines.Add(str);

                                        text_block.Inlines.Add(new LineBreak());
                                    }
                                }
                            }
                                                        
                            else if (result.All(elt => elt.BaseObject is FileSystemInfo))
                            {                                                                                                                                
                                var data_grid = new DataGrid()
                                {
                                    ItemsSource = result.Select(elt => elt.BaseObject),

                                    MaxHeight = 400,

                                    IsReadOnly = true,

                                    GridLinesVisibility = DataGridGridLinesVisibility.None,
                                                                        
                                    BorderThickness = new Thickness(0),
                                    
                                    ColumnHeaderStyle = column_header_style,
                                    
                                    RowHeaderStyle = row_header_style,
                                    
                                    RowStyle = row_style,

                                    AutoGenerateColumns = false,

                                    ContextMenu = new ContextMenu().AddItems(

                                        new MenuItem() { Header = "Explorer" }.AddClick((s, e) =>
                                            Process.Start(((((s as MenuItem).Parent as ContextMenu).PlacementTarget as DataGrid).CurrentItem as FileSystemInfo).FullName)),

                                        new MenuItem() { Header = "Notepad" }.AddClick((s, e) =>
                                            Process.Start("notepad", "\"" + ((((s as MenuItem).Parent as ContextMenu).PlacementTarget as DataGrid).CurrentItem as FileSystemInfo).FullName + "\"")),

                                        new MenuItem() { Header = "vscode" }.AddClick((s, e) =>
                                            new Process()
                                            {
                                                StartInfo = new ProcessStartInfo()
                                                {
                                                    FileName = "code",
                                                    Arguments = "\"" + ((((s as MenuItem).Parent as ContextMenu).PlacementTarget as DataGrid).CurrentItem as FileSystemInfo).FullName + "\"",
                                                    WindowStyle = ProcessWindowStyle.Hidden
                                                }
                                            }.Start()
                                        ),

                                        new MenuItem() { Header = "cd" }.AddClick((s, e) =>
                                        {
                                            var ps_ = PowerShell.Create(); ps_.Runspace = runspace;

                                            ps_.AddScript(String.Format("cd '{0}'", ((((s as MenuItem).Parent as ContextMenu).PlacementTarget as DataGrid).CurrentItem as FileSystemInfo).FullName)).Invoke();

                                            show_current_directory();
                                        }))
                                } 
                                .AddColumns(

                                    new DataGridTextColumn()
                                    {
                                        Header = "LastWriteTime",
                                        Binding = new Binding("LastWriteTime") { StringFormat = "{0:MM/dd/yyyy hh:mm tt}" },
                                        MinWidth = 200
                                    },

                                    make_data_grid_text_column("Length", 100),
                                    make_data_grid_text_column("Name"));

                                text_block.Inlines.Add(data_grid);

                                text_block.Inlines.Add(new LineBreak());
                            }

                            else if (result.All(elt => elt.BaseObject is Process))
                            {
                                var data_grid = new DataGrid()
                                {
                                    // ItemsSource = result.Select(elt => elt.BaseObject),

                                    ItemsSource = result,

                                    MaxHeight = 400,

                                    IsReadOnly = true,
                                    
                                    GridLinesVisibility = DataGridGridLinesVisibility.None,

                                    BorderThickness = new Thickness(0),

                                    ColumnHeaderStyle = column_header_style,

                                    RowHeaderStyle = row_header_style,

                                    RowStyle = row_style,

                                    AutoGenerateColumns = false,

                                    ContextMenu = new ContextMenu().AddItems(
                                        new MenuItem() { Header = "Stop Process" }.AddClick((s, e) =>
                                            (((((s as MenuItem).Parent as ContextMenu).PlacementTarget as DataGrid).CurrentItem as PSObject).BaseObject as Process).Kill()))
                                }
                                .AddColumns(

                                    make_data_grid_text_column("Handles", 100),
                                    make_data_grid_text_column("NPM", 100),
                                    make_data_grid_text_column("PM", 100),
                                    make_data_grid_text_column("WS", 100),

                                    new DataGridTextColumn()
                                    {
                                        Header = "CPU",
                                        Binding = new Binding("TotalProcessorTime.TotalSeconds"),
                                        Width = 100
                                    },

                                    make_data_grid_text_column("Id", 100),
                                    make_data_grid_text_column("SI", 100),
                                    make_data_grid_text_column("ProcessName"));

                                text_block.Inlines.Add(data_grid);
                                                             
                                text_block.Inlines.Add(new LineBreak());
                            }
                            
                            else if (result.All(elt => elt.BaseObject is ServiceController))
                            {
                                var data_grid = new DataGrid()
                                {
                                    // ItemsSource = result,

                                    ItemsSource = result.Select(elt => elt.BaseObject),

                                    MaxHeight = 400,

                                    IsReadOnly = true,

                                    GridLinesVisibility = DataGridGridLinesVisibility.None,

                                    BorderThickness = new Thickness(0),

                                    ColumnHeaderStyle = column_header_style,

                                    RowHeaderStyle = row_header_style,

                                    RowStyle = row_style,

                                    AutoGenerateColumns = false,

                                    ContextMenu = new ContextMenu().AddItems(
                                        new MenuItem() { Header = "Stop" }.AddClick((s, e) =>
                                            (((((s as MenuItem).Parent as ContextMenu).PlacementTarget as DataGrid).CurrentItem as PSObject).BaseObject as ServiceController).Stop()),
                                        new MenuItem() { Header = "Start" }.AddClick((s, e) =>
                                            (((((s as MenuItem).Parent as ContextMenu).PlacementTarget as DataGrid).CurrentItem as PSObject).BaseObject as ServiceController).Start()),
                                        new MenuItem() { Header = "Pause" }.AddClick((s, e) =>
                                            (((((s as MenuItem).Parent as ContextMenu).PlacementTarget as DataGrid).CurrentItem as PSObject).BaseObject as ServiceController).Pause()))
                                }
                                
                                .AddColumns(
                                    make_data_grid_text_column("Status", 100),
                                    make_data_grid_text_column("ServiceName", 300),
                                    make_data_grid_text_column("DisplayName", 500));

                                text_block.Inlines.Add(data_grid);

                                text_block.Inlines.Add(new LineBreak());
                            }

                            else
                                
                                foreach (var elt in result)
                                {
                                    var str = elt.ToString();

                                    text_block.Inlines.Add(str);

                                    text_block.Inlines.Add(new LineBreak());
                                }
                        }
                        catch (Exception exception)
                        {
                            WriteLine(exception);

                            text_block.Inlines.Add(exception.ToString());
                        }

                    scroll_viewer.ScrollToBottom();

                    text_box.Clear();

                    show_current_directory();                    
                }
            };

            dock_panel.AddChildren(text_box);

            dock_panel.AddChildren(scroll_viewer);

            Content = dock_panel;

            Width = 800;
            Height = 400;

            Title = "PowerShell";
        }

        [STAThread]
        public static void Main() => new Application().Run(new MainWindow());
    }
}
