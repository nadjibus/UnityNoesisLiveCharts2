using System;
using System.Collections.Generic;
using ChartsExample;
using Noesis;
using UnityEngine;

public static class VerifyDashboard
{
    public static object Run()
    {
        if (!Application.isPlaying)
        {
            throw new InvalidOperationException("Start Play mode before checking the dashboard.");
        }

        var host = UnityEngine.Object.FindFirstObjectByType<DashboardHost>();
        var view = host.GetComponent<NoesisView>();
        var root = view.Content;
        var buttons = new List<Button>();
        var canvases = new List<LiveChartsCore.Noesis.MotionCanvas>();
        Collect(root, buttons, canvases);
        if (buttons.Count != 3 || canvases.Count != 5)
        {
            throw new Exception("Expected three command buttons and five charts.");
        }

        foreach (var canvas in canvases)
        {
            if (canvas.RenderedFrames < 2 || canvas.LastDrawCount == 0)
            {
                throw new Exception("A chart is not rendering.");
            }
        }

        var model = host.ViewModel;
        if (!model.IsLive)
        {
            model.ToggleLiveCommand.Execute(null);
        }

        Click(view, root, buttons[0]);
        if (model.IsLive)
        {
            throw new Exception("Pause button did not execute its bound command.");
        }

        var revision = model.Revision;
        model.Tick(10);
        if (model.Revision != revision)
        {
            throw new Exception("Data changed while paused.");
        }

        Click(view, root, buttons[1]);
        if (model.Revision != revision + 1)
        {
            throw new Exception("Randomize button did not update the data.");
        }

        var wasDark = model.IsDark;
        Click(view, root, buttons[2]);
        if (model.IsDark == wasDark)
        {
            throw new Exception("Theme button did not execute its bound command.");
        }

        return new { Charts = canvases.Count, model.Revision, model.IsDark, model.IsLive, Frame = Time.frameCount };
    }

    private static void Click(NoesisView view, FrameworkElement root, Button button)
    {
        var point = new Noesis.Vector4(button.ActualWidth / 2, button.ActualHeight / 2, 0, 1) * button.TransformToAncestor(root);
        view.MouseMove((int)point.X, (int)point.Y);
        view.MouseButtonDown((int)point.X, (int)point.Y, MouseButton.Left);
        view.MouseButtonUp((int)point.X, (int)point.Y, MouseButton.Left);
    }

    private static void Collect(DependencyObject node, List<Button> buttons, List<LiveChartsCore.Noesis.MotionCanvas> canvases)
    {
        if (node is Button button)
        {
            buttons.Add(button);
        }

        if (node is LiveChartsCore.Noesis.MotionCanvas canvas)
        {
            canvases.Add(canvas);
        }

        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
        {
            Collect(VisualTreeHelper.GetChild(node, index), buttons, canvases);
        }
    }
}
