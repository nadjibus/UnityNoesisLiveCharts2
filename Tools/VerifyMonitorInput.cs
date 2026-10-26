using System;
using System.Collections.Generic;
using System.IO;
using ChartsExample;
using Noesis;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public static class VerifyMonitorInput
{
    private static readonly string ResultPath = "Logs/monitor-input-check.json";

    public static string Run()
    {
        if (!Application.isPlaying)
        {
            throw new InvalidOperationException("Start the world scene before checking monitor input.");
        }

        var host = UnityEngine.Object.FindFirstObjectByType<DashboardHost>();
        var player = UnityEngine.Object.FindFirstObjectByType<FirstPersonController>();
        var pointer = host.GetComponent<RenderTexturePointer>();
        if (player == null || pointer == null)
        {
            throw new InvalidOperationException("The world scene must contain the player and monitor input.");
        }

        player.SetInteraction(true);
        var view = host.GetComponent<NoesisView>();
        var buttons = new List<Button>();
        CollectButtons(view.Content, buttons);
        var model = host.ViewModel;
        if (!model.IsLive)
        {
            model.ToggleLiveCommand.Execute(null);
        }

        var originalDark = model.IsDark;
        var revision = model.Revision;
        var positions = new UnityEngine.Vector2[buttons.Count];
        for (var index = 0; index < buttons.Count; index++)
        {
            var button = buttons[index];
            var pixel = new Noesis.Vector4(button.ActualWidth / 2, button.ActualHeight / 2, 0, 1) * button.TransformToAncestor(view.Content);
            var local = new Vector3(pixel.X / view.Texture.width - 0.5f, 0.5f - pixel.Y / view.Texture.height, 0);
            var world = host.transform.TransformPoint(local);
            var screen = Camera.main.WorldToScreenPoint(world);
            if (!Physics.Raycast(Camera.main.ScreenPointToRay(screen), out var hit) || hit.collider.gameObject != host.gameObject)
            {
                throw new InvalidOperationException("A monitor button is occluded or outside the camera view.");
            }

            positions[index] = screen;
        }

        var frame = Time.frameCount;
        var phase = 0;
        var started = EditorApplication.timeSinceStartup;
        EditorApplication.CallbackFunction step = null;
        step = () =>
        {
            if (!Application.isPlaying || EditorApplication.timeSinceStartup - started > 15)
            {
                EditorApplication.update -= step;
                SaveResult(false, "Play mode ended or the check timed out.");
                return;
            }

            if (Time.frameCount < frame + 3)
            {
                return;
            }

            frame = Time.frameCount;
            if (phase < 6)
            {
                if (phase == 2)
                {
                    revision = model.Revision;
                }

                var state = new MouseState { position = positions[phase / 2] };
                if (phase % 2 == 0)
                {
                    state = state.WithButton(UnityEngine.InputSystem.LowLevel.MouseButton.Left);
                }

                InputSystem.QueueStateEvent(UnityEngine.InputSystem.Mouse.current, state);
                phase++;
                return;
            }

            EditorApplication.update -= step;
            var passed = !model.IsLive && model.Revision == revision + 1 && model.IsDark != originalDark;
            SaveResult(passed, "Pause/resume, randomize and theme buttons tested through screen rays and mouse events.");
        };
        EditorApplication.update += step;
        return "Monitor input check started; result: " + ResultPath;
    }

    private static void CollectButtons(DependencyObject node, List<Button> buttons)
    {
        if (node is Button button)
        {
            buttons.Add(button);
        }

        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
        {
            CollectButtons(VisualTreeHelper.GetChild(node, index), buttons);
        }
    }

    private static void SaveResult(bool passed, string message)
    {
        Directory.CreateDirectory("Logs");
        File.WriteAllText(ResultPath, "{\"passed\":" + (passed ? "true" : "false") + ",\"gameHeight\":" + Screen.height + ",\"message\":\"" + message + "\"}");
    }
}
