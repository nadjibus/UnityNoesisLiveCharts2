using System.ComponentModel;
using Noesis;
using UnityEngine;

namespace ChartsExample
{
    [RequireComponent(typeof(NoesisView))]
    public sealed class DashboardHost : MonoBehaviour
    {
        public NoesisXaml DarkPalette;
        public NoesisXaml LightPalette;

        private FrameworkElement _root;
        private DashboardViewModel _viewModel;

        public DashboardViewModel ViewModel => _viewModel;

        private void Start()
        {
            _root = GetComponent<NoesisView>().Content;
            _viewModel = new DashboardViewModel();
            _viewModel.PropertyChanged += OnPropertyChanged;
            _root.DataContext = _viewModel;
            ApplyPalette();
        }

        private void Update()
        {
            _viewModel?.Tick(Time.unscaledDeltaTime);
        }

        private void OnDestroy()
        {
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= OnPropertyChanged;
            }

            if (_root != null)
            {
                _root.DataContext = null;
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(DashboardViewModel.IsDark))
            {
                ApplyPalette();
            }
        }

        private void ApplyPalette()
        {
            var palette = _viewModel.IsDark ? DarkPalette : LightPalette;
            _root.Resources.MergedDictionaries.Clear();
            _root.Resources.MergedDictionaries.Add((ResourceDictionary)palette.Load());
        }
    }
}
