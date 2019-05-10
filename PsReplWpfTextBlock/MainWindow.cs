using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dharmatech.WpfExtensionMethods;

namespace PsReplWpfTextBlock
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            var dock_panel = new DockPanel();
            
            var scroll_viewer = new ScrollViewer();

            var text_block = new TextBlock()
            {
                FontFamily = new FontFamily("Lucida Console"),
                Background = new SolidColorBrush(Color.FromRgb(1, 36, 86)),
                Foreground = new SolidColorBrush(Colors.WhiteSmoke)
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

            show_current_directory();
            
            text_box.KeyDown += (sender, event_args) => 
            {
                if (event_args.Key == Key.Return)
                {
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
                            foreach (var elt in result)
                            {
                                if (elt.BaseObject is DirectoryInfo info)
                                {                                                                        
                                    text_block.Inlines.Add(
                                        new Button()
                                        {
                                            Content = info.Name,
                                            ContextMenu = new ContextMenu().AddItems(new MenuItem() { Header = "Explorer" }.AddClick((s, e) => System.Diagnostics.Process.Start(info.FullName)))
                                        }
                                        .AddClick((s, e) =>
                                        {
                                            var ps_ = PowerShell.Create();

                                            ps_.Runspace = runspace;

                                            ps_.AddScript(String.Format("cd '{0}'", info.FullName)).Invoke();

                                            show_current_directory();
                                        }));

                                    text_block.Inlines.Add(new LineBreak());
                                }
                                else if (elt.BaseObject is FileInfo file_info && (file_info.Extension == ".jpg" || file_info.Extension == ".png"))
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
                                else if (elt.BaseObject is System.Diagnostics.Process process)
                                {
                                    text_block.Inlines.Add(
                                        new Button()
                                        {
                                            Content = process.ProcessName,
                                            ContextMenu = new ContextMenu().AddItems(new MenuItem() { Header = "Stop Process" }.AddClick((s, e) => process.Kill()))
                                        }
                                        .AddClick((s, e) => { }));

                                    text_block.Inlines.Add(new LineBreak());
                                }
                                else if (elt.BaseObject is System.ServiceProcess.ServiceController service_controller)
                                {
                                    text_block.Inlines.Add(
                                        new Button()
                                        {
                                            Content = String.Format("{0} ({1})", service_controller.DisplayName, service_controller.Status.ToString()),
                                            ContextMenu = new ContextMenu().AddItems(
                                                new MenuItem() { Header = "Start Service" }.AddClick((s, e) => service_controller.Start()),
                                                new MenuItem() { Header = "Stop Service" }.AddClick((s, e) => service_controller.Stop()),
                                                new MenuItem() { Header = "Pause Service" }.AddClick((s, e) => service_controller.Pause()))
                                        }
                                        .AddClick((s, e) => { }));

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
                        catch (Exception exception)
                        {
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

            Title = "PowerShell";
        }

        [STAThread] public static void Main()
        {
            new Application().Run(new MainWindow());
        }
    }
}
