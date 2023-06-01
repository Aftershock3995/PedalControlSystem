using RacingSimPedalsReader;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

public class GraphControl : Control
{
    private List<PointF> dataPoints;
    private Pen linePen;
    private Pen markerPen;
    private int selectedPointIndex;
    private bool isDragging;
    private bool pedal1Graph;
    private bool pedal2Graph;
    private bool pedal3Graph;
    private float pedalPositionX;  // X-coordinate of the pedal position
    private bool showPedalPosition;  // Flag to indicate whether to show the pedal position line
    public int pedal1Position { get; set; }
    public int pedal2Position { get; set; }
    public int pedal3Position { get; set; }
    public int[] PedalGraphs { get; set; }

    private const int NumGridLines = 10; // Number of grid lines in each direction
    private const float GridSpacing = 0.1f; // Spacing between grid lines as a fraction of graph size

    private float minX = 0f; // Minimum X-coordinate of the grid
    private float maxX = 1f; // Maximum X-coordinate of the grid
    private float minY = 0f; // Minimum Y-coordinate of the grid
    private float maxY = 1f; // Maximum Y-coordinate of the grid

    public GraphControl()
    {
        dataPoints = new List<PointF>();
        linePen = new Pen(Color.Black, 2f);
        markerPen = new Pen(Color.Red, 2.5f);
        selectedPointIndex = -1;
        isDragging = false;
        pedal1Graph = false;
        pedal2Graph = false;
        pedal3Graph = false;

    // Enable mouse events
        SetStyle(ControlStyles.Selectable, true);
        SetStyle(ControlStyles.UserMouse, true);
        SetStyle(ControlStyles.StandardClick, true);
        SetStyle(ControlStyles.StandardDoubleClick, true);
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        DoubleBuffered = true;

        // Register mouse event handlers
        MouseDown += GraphControl_MouseDown;
        MouseMove += GraphControl_MouseMove;
        MouseUp += GraphControl_MouseUp;
        GotFocus += GraphControl_GotFocus;
        LostFocus += GraphControl_LostFocus;
        pedalPositionX = 0f;  // Set the initial pedal position X-coordinate
        showPedalPosition = false;  // Initially, do not show the pedal position line
    }

    public void RefreshGraph(List<PointF> newDataPoints)
    {
        dataPoints = newDataPoints;
        Invalidate();
    }

    public int Pedal1GraphActive
    {
        get { return pedal1Graph ? 1 : 0; }
        set
        {
            bool isActive = (value != 0);

            if (pedal1Graph != isActive)
            {
                pedal1Graph = isActive;
                if (!isActive && selectedPointIndex != -1)
                {
                    selectedPointIndex = -1;
                    isDragging = false;
                }
                Invalidate();
            }
        }
    }

    public int Pedal2GraphActive
    {
        get { return pedal2Graph ? 1 : 0; }
        set
        {
            bool isActive = (value != 0);

            if (pedal2Graph != isActive)
            {
                pedal2Graph = isActive;
                if (!isActive && selectedPointIndex != -1)
                {
                    selectedPointIndex = -1;
                    isDragging = false;
                }
                Invalidate();
            }
        }
    }

    public int Pedal3GraphActive
    {
        get { return pedal3Graph ? 1 : 0; }
        set
        {
            bool isActive = (value != 0);

            if (pedal3Graph != isActive)
            {
                pedal3Graph = isActive;
                if (!isActive && selectedPointIndex != -1)
                {
                    selectedPointIndex = -1;
                    isDragging = false;
                }
                Invalidate();
            }
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        float scaleX = Width / (maxX - minX);
        float scaleY = Height / (maxY - minY);

        if (dataPoints.Count > 0)
        {
            PointF prevPoint = PointF.Empty;

            for (int i = 0; i < dataPoints.Count; i++)
            {
                PointF point = dataPoints[i];
                float x = (point.X - minX) * scaleX;
                float y = Height - (point.Y - minY) * scaleY;

                if (prevPoint != PointF.Empty)
                    graphics.DrawLine(linePen, prevPoint, new PointF(x, y));

                if (isDragging && selectedPointIndex == i)
                {
                    graphics.DrawEllipse(markerPen, x - 4, y - 4, 8, 8);
                }
                else
                {
                    graphics.FillEllipse(Brushes.White, x - 4, y - 4, 8, 8);
                    graphics.DrawEllipse(markerPen, x - 4, y - 4, 8, 8);
                }

                prevPoint = new PointF(x, y);
            }

            // Draw pedal position line
            float pedalX = (pedal1Position - minX) * scaleX;
            graphics.DrawLine(markerPen, pedalX, 0, pedalX, Height);
        }
    }

    private void DrawBackgroundGrid(Graphics graphics)
    {
        float graphWidth = Width - 1;
        float graphHeight = Height - 1;

        float gridSpacingX = graphWidth * GridSpacing;
        float gridSpacingY = graphHeight * GridSpacing;

        Pen gridPen = new(Color.DarkGray, 1f);

        // Draw vertical grid lines
        for (int i = 0; i <= NumGridLines; i++)
        {
            float x = i * gridSpacingX;
            graphics.DrawLine(gridPen, x, 0, x, graphHeight);
        }

        // Draw horizontal grid lines
        for (int i = 0; i <= NumGridLines; i++)
        {
            float y = i * gridSpacingY;
            graphics.DrawLine(gridPen, 0, y, graphWidth, y);
        }

        // Draw squares at grid intersections
        Brush squareBrush = new SolidBrush(Color.DarkGray);
        float squareSize = 3f;

        for (int i = 0; i <= NumGridLines; i++)
        {
            for (int j = 0; j <= NumGridLines; j++)
            {
                float x = i * gridSpacingX;
                float y = j * gridSpacingY;
                graphics.FillRectangle(squareBrush, x - squareSize / 2, y - squareSize / 2, squareSize, squareSize);
            }
        }
    }

    private void DrawGraph(Graphics graphics)
    {
        int graphWidth = ClientSize.Width;
        int graphHeight = ClientSize.Height;

        PointF[] curvePoints = new PointF[dataPoints.Count]; // Initialize curvePoints with the appropriate capacity

        // Store curve points
        for (int i = 0; i < dataPoints.Count; i++)
        {
            curvePoints[i] = PointToGraphCoordinates(dataPoints[i], graphWidth, graphHeight);
        }

        // Draw curve
        if (dataPoints.Count > 1)
        {
            graphics.DrawCurve(linePen, curvePoints);
        }

        // Draw markers
        foreach (PointF graphPoint in curvePoints)
        {
            graphics.DrawEllipse(markerPen, graphPoint.X - 3, graphPoint.Y - 3, 6, 6);
        }

        // Draw pedal position lines
        if (pedal1Graph && Pedal1GraphActive == 1)
        {
            PointF pedal1PositionPoint = new PointF(graphWidth * (pedal1Position / 100f), graphHeight);
            PointF pedal1PositionGraphPoint = PointToGraphCoordinates(pedal1PositionPoint, graphWidth, graphHeight);
            graphics.DrawLine(linePen, pedal1PositionGraphPoint.X, 0, pedal1PositionGraphPoint.X, graphHeight);
        }

        if (pedal2Graph && Pedal2GraphActive == 1)
        {
            PointF pedal2PositionPoint = new PointF(graphWidth * (pedal2Position / 100f), graphHeight);
            PointF pedal2PositionGraphPoint = PointToGraphCoordinates(pedal2PositionPoint, graphWidth, graphHeight);
            graphics.DrawLine(linePen, pedal2PositionGraphPoint.X, 0, pedal2PositionGraphPoint.X, graphHeight);
        }

        if (pedal3Graph && Pedal3GraphActive == 1)
        {
            PointF pedal3PositionPoint = new PointF(graphWidth * (pedal3Position / 100f), graphHeight);
            PointF pedal3PositionGraphPoint = PointToGraphCoordinates(pedal3PositionPoint, graphWidth, graphHeight);
            graphics.DrawLine(linePen, pedal3PositionGraphPoint.X, 0, pedal3PositionGraphPoint.X, graphHeight);
        }

        // Draw borders
        if (pedal1Graph && Pedal1GraphActive == 1)
        {
            DrawBorder(graphics, Color.LightGray, 2.5f);
        }

        if (pedal2Graph && Pedal2GraphActive == 1)
        {
            DrawBorder(graphics, Color.LightGray, 2.5f);
        }

        if (pedal3Graph && Pedal3GraphActive == 1)
        {
            DrawBorder(graphics, Color.LightGray, 2.5f);
        }
    }

    private PointF PointToGraphCoordinates(PointF point, float graphWidth, float graphHeight)
    {
        float x = point.X * graphWidth;
        float y = (1f - point.Y) * graphHeight;
        return new PointF(x, y);
    }


    private float CalculateDistance(PointF point1, PointF point2)
    {
        float deltaX = point2.X - point1.X;
        float deltaY = point2.Y - point1.Y;
        return (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
    }

    private int FindClosestPointIndex(PointF point, float graphWidth, float graphHeight)
    {
        float thresholdDistance = 8f; // Adjust this value as needed

        for (int i = 0; i < dataPoints.Count; i++)
        {
            PointF graphPoint = PointToGraphCoordinates(dataPoints[i], graphWidth, graphHeight);
            float distance = CalculateDistance(point, graphPoint);

            if (distance < thresholdDistance)
                return i;
        }

        return -1;
    }

    private void DrawBorder(Graphics graphics, Color color, float penWidth)
    {
        using (Pen borderPen = new(color, penWidth))
        {
            graphics.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
        }
    }

    private void GraphControl_MouseDown(object sender, MouseEventArgs e)
    {
        float graphWidth = Width - 1;
        float graphHeight = Height - 1;

        selectedPointIndex = FindClosestPointIndex(e.Location, graphWidth, graphHeight);

        if (selectedPointIndex != -1)
        {
            isDragging = true;
            Capture = true;
        }
    }

    private void GraphControl_MouseMove(object sender, MouseEventArgs e)
    {
        if (isDragging && selectedPointIndex != -1)
        {
            float graphWidth = Width - 1;
            float graphHeight = Height - 1;

            PointF normalizedPoint = GraphToNormalizedCoordinates(e.Location, graphWidth, graphHeight);

            // Check if the normalized point is within the grid bounds with buffer
            if (normalizedPoint.X >= minX + 0.02f && normalizedPoint.X <= maxX - 0.02f &&
                normalizedPoint.Y >= minY + 0.03f && normalizedPoint.Y <= maxY - 0.03f)
            {
                dataPoints[selectedPointIndex] = normalizedPoint;
                Invalidate();
            }
        }
    }

    private void GraphControl_MouseUp(object sender, MouseEventArgs e)
    {
        isDragging = false;
        Capture = false;
    }

    private void GraphControl_GotFocus(object sender, EventArgs e)
    {
        pedal1Graph = true;
        pedal2Graph = true;
        pedal3Graph = true;
        Invalidate();
    }

    private void GraphControl_LostFocus(object sender, EventArgs e)
    {
        pedal1Graph = false;
        pedal2Graph = false;
        pedal3Graph = false;
        Invalidate();
    }

    private PointF GraphToNormalizedCoordinates(PointF point, float graphWidth, float graphHeight)
    {
        float x = point.X / graphWidth;
        float y = 1f - (point.Y / graphHeight);
        return new PointF(x, y);
    }

    public void SetGridBounds(float minX, float maxX, float minY, float maxY)
    {
        this.minX = minX;
        this.maxX = maxX;
        this.minY = minY;
        this.maxY = maxY;
    }

    // New method to add data point
    public void AddDataPoint(PointF point)
    {
        float graphWidth = Width - 1;
        float graphHeight = Height - 1;

        PointF normalizedPoint = GraphToNormalizedCoordinates(point, graphWidth, graphHeight);

        // Check if the normalized point is within the grid bounds
        if (normalizedPoint.X >= minX && normalizedPoint.X <= maxX &&
            normalizedPoint.Y >= minY && normalizedPoint.Y <= maxY)
        {
            dataPoints.Add(normalizedPoint);
            Invalidate();
        }
    }
    public void UpdatePedalPosition(float positionX)
    {
        pedalPositionX = positionX;
        showPedalPosition = true;
        Invalidate();
    }
    public void RefreshGraph(List<PointF> newDataPoints, int pedalPosition)
    {
        dataPoints = newDataPoints;
        pedalPosition = pedalPosition;
        Invalidate();
    }
}
