using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Layout;
using MiniMvvm;

namespace VirtualizationImages.ViewModels
{
    internal class MainWindowViewModel : ViewModelBase
    {
        private int _itemCount = 20;
        private string _newItemString = "New Item";
        private int _newItemIndex;
        private AvaloniaList<PageViewModel> _items;
        private string _prefix = "Item";
        private ScrollBarVisibility _horizontalScrollBarVisibility = ScrollBarVisibility.Auto;
        private ScrollBarVisibility _verticalScrollBarVisibility = ScrollBarVisibility.Auto;
        private Orientation _orientation = Orientation.Vertical;
        private ItemVirtualizationMode _virtualizationMode = ItemVirtualizationMode.Simple;

        public MainWindowViewModel()
        {
            this.WhenAnyValue(x => x.ItemCount).Subscribe(ResizeItems);
            Selection.WhenAnyValue(x => x.SelectedIndex).Subscribe(SelectItem);
            RecreateCommand = MiniCommand.Create(() => Recreate());

            AddItemCommand = MiniCommand.Create(() => AddItem());

            RemoveItemCommand = MiniCommand.Create(() => Remove());

            SelectFirstCommand = MiniCommand.Create(() => SelectItem(0));

            SelectLastCommand = MiniCommand.Create(() => SelectItem(Items.Count - 1));
        }

        public MiniCommand AddItemCommand { get; private set; }
        public MiniCommand RecreateCommand { get; private set; }
        public MiniCommand RemoveItemCommand { get; private set; }
        public MiniCommand SelectFirstCommand { get; private set; }
        public MiniCommand SelectLastCommand { get; private set; }

        public Orientation Orientation
        {
            get { return _orientation; }
            set { this.RaiseAndSetIfChanged(ref _orientation, value); }
        }

        public IEnumerable<Orientation> Orientations =>
            Enum.GetValues(typeof(Orientation)).Cast<Orientation>();

        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get { return _horizontalScrollBarVisibility; }
            set { this.RaiseAndSetIfChanged(ref _horizontalScrollBarVisibility, value); }
        }

        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get { return _verticalScrollBarVisibility; }
            set { this.RaiseAndSetIfChanged(ref _verticalScrollBarVisibility, value); }
        }

        public IEnumerable<ScrollBarVisibility> ScrollBarVisibilities =>
            Enum.GetValues(typeof(ScrollBarVisibility)).Cast<ScrollBarVisibility>();

        public ItemVirtualizationMode VirtualizationMode
        {
            get { return _virtualizationMode; }
            set { this.RaiseAndSetIfChanged(ref _virtualizationMode, value); }
        }

        public IEnumerable<ItemVirtualizationMode> VirtualizationModes =>
            Enum.GetValues(typeof(ItemVirtualizationMode)).Cast<ItemVirtualizationMode>();

        
        public EventHandler<EventArgs> ZoomEventHandler;

        private double _zoom = 0.5;
        public double Zoom
        {
            get => _zoom;
            set
            {
                if (value > 0)
                {
                    _zoom = value;
                    RaisePropertyChanged();
                    ZoomEventHandler?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void ResizeItems(int count)
        {
            if (Items == null)
            {
                var items = Enumerable.Range(0, count)
                    .Select(x => new PageViewModel(x));
                Items = new AvaloniaList<PageViewModel>(items);
            }
            else if (count > Items.Count)
            {
                var items = Enumerable.Range(Items.Count, count - Items.Count)
                    .Select(x => new PageViewModel(x));
                Items.AddRange(items);
            }
            else if (count < Items.Count)
            {
                Items.RemoveRange(count, Items.Count - count);
            }
        }

        public string NewItemString
        {
            get { return _newItemString; }
            set { this.RaiseAndSetIfChanged(ref _newItemString, value); }
        }

        public int ItemCount
        {
            get { return _itemCount; }
            set { this.RaiseAndSetIfChanged(ref _itemCount, value); }
        }

        private bool _autoScroll = true;
        public bool AutoScroll
        {
            get { return _autoScroll; }
            set { this.RaiseAndSetIfChanged(ref _autoScroll, value); }
        }

        public SelectionModel<PageViewModel> Selection { get; } = new SelectionModel<PageViewModel>();

        public AvaloniaList<PageViewModel> Items
        {
            get { return _items; }
            private set { this.RaiseAndSetIfChanged(ref _items, value); }
        }
        
        private void AddItem()
        {
            var index = Items.Count;

            if (Selection.SelectedItems.Count > 0)
            {
                index = Selection.SelectedIndex;
            }

            Items.Insert(index, new PageViewModel(_newItemIndex++, NewItemString));
        }

        private void Remove()
        {
            if (Selection.SelectedItems.Count > 0)
            {
                Items.RemoveAll(Selection.SelectedItems.ToList());
            }
        }

        private void Recreate()
        {
            _prefix = _prefix == "Item" ? "Recreated" : "Item";
            var items = Enumerable.Range(0, _itemCount)
                .Select(x => new PageViewModel(x, _prefix));
            Items = new AvaloniaList<PageViewModel>(items);
        }

        private void SelectItem(int index)
        {
            Selection.SelectedIndex = index;
        }


        public void ZoomIn()
        {
            if (Math.Round(Zoom, 2, MidpointRounding.ToEven) >= 64)
            {
                return;
            }

            if (Math.Round(Zoom, 2, MidpointRounding.ToEven) >= 1)
            {
                Zoom += 0.1d;
            }
            else if (Math.Round(Zoom, 2, MidpointRounding.ToEven) >= 0.6)
            {
                Zoom += 0.05d;
            }
            else if (Math.Round(Zoom, 2, MidpointRounding.ToEven) >= 0.20)
            {
                Zoom += 0.02d;
            }
            else if (Math.Round(Zoom, 2, MidpointRounding.ToEven) > 0 /*&& Math.Round(Zoom, 2, MidpointRounding.ToEven) < 0.2*/)
            {
                Zoom += 0.01d;
            }

        }

        public void ZoomOut()
        {
            if (Math.Round(Zoom, 2, MidpointRounding.ToEven) <= 0.02)
            {
                return;
            }
            
            if (Math.Round(Zoom, 2, MidpointRounding.ToEven) >= 1.1)
            {
                Zoom -= 0.1d;
            }
            else if (Math.Round(Zoom, 2, MidpointRounding.ToEven) >= 0.65)
            {
                Zoom -= 0.05d;
            }
            else if (Math.Round(Zoom, 2, MidpointRounding.ToEven) > 0.22)
            {
                Zoom -= 0.02d;
            }
            else if (Math.Round(Zoom, 2, MidpointRounding.ToEven) > 0 /*&& Math.Round(Zoom, 2, MidpointRounding.ToEven) <= 0.2*/)
            {
                Zoom -= 0.01d;
            }

        }


    }


}
