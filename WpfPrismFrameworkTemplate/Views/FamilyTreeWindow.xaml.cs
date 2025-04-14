using System.Windows;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using WpfPrismFrameworkTemplate.ViewModels;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Shapes;
using WpfPrismFrameworkTemplate.Model;

namespace WpfPrismFrameworkTemplate.Views
{
    /// <summary>
    /// Interaction logic for FamilyTreeWindow
    /// </summary>
    public partial class FamilyTreeWindow : UserControl
    {
        private bool _isDragging;
        private Point _startPoint;
        private TextBlock _draggedElement;
        private bool _isCanvasInternalDrag;
        private Point _originalPosition;

        // 连线相关变量
        private TextBlock _sourceElement;
        private bool _isConnecting;
        private Line _previewLine;
        // 将原有的Dictionary<string, List<Line>>替换为更完善的结构
        private Dictionary<string, List<ConnectionLineInfo>> _connectionLinesInfo = new Dictionary<string, List<ConnectionLineInfo>>();

        public FamilyTreeWindow()
        {
            InitializeComponent();
            _connectionLinesInfo = new Dictionary<string, List<ConnectionLineInfo>>();
        }

        private void TextBlock_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_isDragging) return;

            Point currentPosition = e.GetPosition(null);
            Vector diff = _startPoint - currentPosition;

            // 如果移动超过了拖拽阈值，开始拖拽操作
            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                 Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                TextBlock textBlock = sender as TextBlock;
                if (textBlock == null) return;

                // 获取ViewModel
                FamilyTreeWindowViewModel viewModel = DataContext as FamilyTreeWindowViewModel;
                if (viewModel == null) return;

                // 创建拖拽数据
                DataObject dragData = new DataObject();
                dragData.SetData("DraggableTextContent", viewModel.DraggableText);

                // 开始拖拽操作
                DragDrop.DoDragDrop(textBlock, dragData, DragDropEffects.Copy);

                _isDragging = false;
                textBlock.ReleaseMouseCapture();
            }
        }

        private void TextBlock_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
            _isDragging = true;

            // 获取焦点以确保接收后续事件
            ((TextBlock)sender).CaptureMouse();
        }

        private void TextBlock_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            ((TextBlock)sender).ReleaseMouseCapture();
        }

        private void Canvas_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("DraggableTextContent"))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void Canvas_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("DraggableTextContent"))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void Canvas_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("DraggableTextContent"))
            {
                // 获取拖拽的文本内容
                string droppedText = e.Data.GetData("DraggableTextContent") as string;

                // 创建新的TextBlock和外层Border (用于右键菜单和可视化标识)
                TextBlock newTextBlock = new TextBlock
                {
                    Text = droppedText,
                    Background = Brushes.LightGreen,
                    Padding = new Thickness(10),
                    Tag = Guid.NewGuid().ToString() // 用于标识唯一控件
                };

                // 为TextBlock添加右键菜单
                newTextBlock.ContextMenu = new ContextMenu();
                MenuItem connectMenuItem = new MenuItem { Header = "开始连线" };
                connectMenuItem.Click += (s, args) => StartConnection(newTextBlock);
                newTextBlock.ContextMenu.Items.Add(connectMenuItem);

                // 为新创建的TextBlock添加鼠标事件处理，使其能在Canvas内拖动
                newTextBlock.MouseLeftButtonDown += CanvasElement_MouseLeftButtonDown;
                newTextBlock.MouseMove += CanvasElement_MouseMove;
                newTextBlock.MouseLeftButtonUp += CanvasElement_MouseLeftButtonUp;

                // 当连线模式激活时，单击事件用于完成连线
                newTextBlock.MouseLeftButtonDown += (s, args) => {
                    if (_isConnecting && _sourceElement != null && _sourceElement != newTextBlock)
                    {
                        CompleteConnection(newTextBlock);
                        args.Handled = true;
                    }
                };

                // 获取放置的坐标位置
                Point dropPosition = e.GetPosition(DestinationCanvas);

                // 将新的TextBlock添加到Canvas中
                DestinationCanvas.Children.Add(newTextBlock);

                // 设置Canvas的附加属性，确定位置
                Canvas.SetLeft(newTextBlock, dropPosition.X);
                Canvas.SetTop(newTextBlock, dropPosition.Y);

                // 初始化连线字典
                // 初始化连接线信息字典
                _connectionLinesInfo[newTextBlock.Tag.ToString()] = new List<ConnectionLineInfo>();

                // 通知ViewModel
                FamilyTreeWindowViewModel viewModel = DataContext as FamilyTreeWindowViewModel;
                viewModel?.NotifyTextBlockDropped(droppedText, dropPosition, newTextBlock.Tag.ToString());

                e.Handled = true;
            }
        }

        private void CanvasElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 如果正在连线模式，则跳过拖拽
            if (_isConnecting) return;

            _draggedElement = sender as TextBlock;
            if (_draggedElement != null)
            {
                _isCanvasInternalDrag = true;
                _isDragging = true;
                _startPoint = e.GetPosition(DestinationCanvas);

                // 记录原始位置
                _originalPosition = new Point(
                    Canvas.GetLeft(_draggedElement),
                    Canvas.GetTop(_draggedElement));

                // 捕获鼠标
                _draggedElement.CaptureMouse();
                e.Handled = true;
            }
        }

        private void CanvasElement_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isCanvasInternalDrag && _isDragging && _draggedElement != null)
            {
                Point currentPosition = e.GetPosition(DestinationCanvas);

                // 计算位移
                double offsetX = currentPosition.X - _startPoint.X;
                double offsetY = currentPosition.Y - _startPoint.Y;

                // 更新元素位置
                double newLeft = _originalPosition.X + offsetX;
                double newTop = _originalPosition.Y + offsetY;

                Canvas.SetLeft(_draggedElement, newLeft);
                Canvas.SetTop(_draggedElement, newTop);

                // 更新相关的连线
                UpdateConnectionLines(_draggedElement);

                e.Handled = true;
            }
        }

        private void CanvasElement_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isCanvasInternalDrag && _draggedElement != null)
            {
                // 释放鼠标捕获
                _draggedElement.ReleaseMouseCapture();

                // 更新ViewModel中的位置信息
                FamilyTreeWindowViewModel viewModel = DataContext as FamilyTreeWindowViewModel;
                viewModel?.UpdateTextBlockPosition(
                    _draggedElement.Text,
                    Canvas.GetLeft(_draggedElement),
                    Canvas.GetTop(_draggedElement));

                // 重置状态
                _isDragging = false;
                _isCanvasInternalDrag = false;
                _draggedElement = null;

                e.Handled = true;
            }
        }

        // 开始连线过程
        private void StartConnection(TextBlock source)
        {
            _sourceElement = source;
            _isConnecting = true;

            // 创建预览线
            _previewLine = new Line
            {
                Stroke = Brushes.Blue,
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection(new double[] { 4, 2 })
            };

            // 设置线的起点
            Point sourceCenter = GetElementCenter(source);
            _previewLine.X1 = sourceCenter.X;
            _previewLine.Y1 = sourceCenter.Y;
            _previewLine.X2 = sourceCenter.X;
            _previewLine.Y2 = sourceCenter.Y;

            // 将预览线添加到Canvas
            DestinationCanvas.Children.Add(_previewLine);

            // 添加Canvas的鼠标移动事件用于更新预览线
            DestinationCanvas.MouseMove += Canvas_MouseMoveForConnection;

            // 添加Canvas的鼠标右键点击事件用于取消连线
            DestinationCanvas.MouseRightButtonDown += Canvas_MouseRightButtonDownForConnection;

            // 更改鼠标指针
            this.Cursor = Cursors.Cross;
        }

        // 鼠标移动时更新预览线
        private void Canvas_MouseMoveForConnection(object sender, MouseEventArgs e)
        {
            if (_isConnecting && _previewLine != null)
            {
                Point mousePos = e.GetPosition(DestinationCanvas);
                _previewLine.X2 = mousePos.X;
                _previewLine.Y2 = mousePos.Y;
            }
        }

        // 右键点击取消连线
        private void Canvas_MouseRightButtonDownForConnection(object sender, MouseButtonEventArgs e)
        {
            if (_isConnecting)
            {
                CancelConnection();
                e.Handled = true;
            }
        }

        // 取消连线
        private void CancelConnection()
        {
            if (_previewLine != null)
            {
                DestinationCanvas.Children.Remove(_previewLine);
                _previewLine = null;
            }

            // 移除事件处理器
            DestinationCanvas.MouseMove -= Canvas_MouseMoveForConnection;
            DestinationCanvas.MouseRightButtonDown -= Canvas_MouseRightButtonDownForConnection;

            // 重置状态
            _isConnecting = false;
            _sourceElement = null;
            this.Cursor = Cursors.Arrow;
        }

        // 完成连线
        private void CompleteConnection(TextBlock target)
        {
            if (_sourceElement == null || _previewLine == null) return;

            // 删除预览线
            DestinationCanvas.Children.Remove(_previewLine);

            // 创建实际的连接线
            Line connectionLine = new Line
            {
                Stroke = Brushes.Red,
                StrokeThickness = 2
            };

            // 设置线的起点和终点
            Point sourceCenter = GetElementCenter(_sourceElement);
            Point targetCenter = GetElementCenter(target);

            connectionLine.X1 = sourceCenter.X;
            connectionLine.Y1 = sourceCenter.Y;
            connectionLine.X2 = targetCenter.X;
            connectionLine.Y2 = targetCenter.Y;

            // 设置线的Z轴顺序，确保在TextBlock下方绘制
            Panel.SetZIndex(connectionLine, -1);

            // 将连接线添加到Canvas
            DestinationCanvas.Children.Add(connectionLine);

            // 存储连接线信息
            string sourceId = _sourceElement.Tag.ToString();
            string targetId = target.Tag.ToString();

            // 创建连接线信息对象
            ConnectionLineInfo lineInfo = new ConnectionLineInfo
            {
                Line = connectionLine,
                SourceId = sourceId,
                TargetId = targetId
            };

            // 初始化字典条目（如果需要）
            if (!_connectionLinesInfo.ContainsKey(sourceId))
            {
                _connectionLinesInfo[sourceId] = new List<ConnectionLineInfo>();
            }
            if (!_connectionLinesInfo.ContainsKey(targetId))
            {
                _connectionLinesInfo[targetId] = new List<ConnectionLineInfo>();
            }

            // 在源和目标的字典中都添加此连接信息
            _connectionLinesInfo[sourceId].Add(lineInfo);
            _connectionLinesInfo[targetId].Add(lineInfo);

            // 将连接信息添加到ViewModel
            FamilyTreeWindowViewModel viewModel = DataContext as FamilyTreeWindowViewModel;
            viewModel?.AddConnection(sourceId, targetId);

            // 重置状态
            _previewLine = null;
            DestinationCanvas.MouseMove -= Canvas_MouseMoveForConnection;
            DestinationCanvas.MouseRightButtonDown -= Canvas_MouseRightButtonDownForConnection;
            _isConnecting = false;
            _sourceElement = null;
            this.Cursor = Cursors.Arrow;
        }

        // 获取元素的中心点
        private Point GetElementCenter(TextBlock element)
        {
            double left = Canvas.GetLeft(element);
            double top = Canvas.GetTop(element);
            double width = element.ActualWidth;
            double height = element.ActualHeight;

            return new Point(left + width / 2, top + height / 2);
        }

        // 更新与特定元素相关的所有连接线
        private void UpdateConnectionLines(TextBlock element)
        {
            if (element == null || element.Tag == null) return;

            string elementId = element.Tag.ToString();
            if (!_connectionLinesInfo.ContainsKey(elementId)) return;

            Point elementCenter = GetElementCenter(element);

            foreach (var lineInfo in _connectionLinesInfo[elementId])
            {
                Line line = lineInfo.Line;

                // 确定此元素是源还是目标
                bool isSource = lineInfo.SourceId == elementId;

                // 查找连接的另一个元素
                TextBlock otherElement = null;
                foreach (UIElement child in DestinationCanvas.Children)
                {
                    if (child is TextBlock tb && tb.Tag != null)
                    {
                        string otherId = tb.Tag.ToString();
                        if ((isSource && otherId == lineInfo.TargetId) ||
                            (!isSource && otherId == lineInfo.SourceId))
                        {
                            otherElement = tb;
                            break;
                        }
                    }
                }

                if (otherElement != null)
                {
                    Point otherCenter = GetElementCenter(otherElement);

                    // 更新连接线端点
                    if (isSource)
                    {
                        // 当前元素是源
                        line.X1 = elementCenter.X;
                        line.Y1 = elementCenter.Y;
                        line.X2 = otherCenter.X;
                        line.Y2 = otherCenter.Y;
                    }
                    else
                    {
                        // 当前元素是目标
                        line.X1 = otherCenter.X;
                        line.Y1 = otherCenter.Y;
                        line.X2 = elementCenter.X;
                        line.Y2 = elementCenter.Y;
                    }
                }
            }
        }
    }
}
