﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

using TouchTracking;

namespace TouchTrackingEffectDemos
{
    public partial class FingerPaintPage : ContentPage
    {
        Dictionary<long, FingerPaintPolyline> inProgressPolylines = new Dictionary<long, FingerPaintPolyline>();
        List<FingerPaintPolyline> completedPolylines = new List<FingerPaintPolyline>();

        SKPaint paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round
        };

        public FingerPaintPage()
        {
            InitializeComponent();
        }

        void OnClearButtonClicked(object sender, EventArgs args)
        {
            completedPolylines.Clear(); 
            canvasView.InvalidateSurface();
        }

        void OnTouchEffectTouchAction(object sender, TouchActionEventArgs args)
        {
        //    SKCanvasView canvasView = sender as SKCanvasView;



            TouchEffect touchEffect = (TouchEffect)canvasViewGrid.Effects.First(e => e is TouchEffect);

            switch (args.Type)
            {
                case TouchActionType.Entered:
                case TouchActionType.Pressed:
                    if (args.IsInContact)
                    {
                        if (!inProgressPolylines.ContainsKey(args.Id))
                        {
                            touchEffect.Capture = true;

                            Color strokeColor = (Color)typeof(Color).GetRuntimeField(colorPicker.Items[colorPicker.SelectedIndex]).GetValue(null);
                            float strokeWidth = ConvertToPixel(new float[] { 1, 2, 5, 10, 20 }[widthPicker.SelectedIndex]);

                            FingerPaintPolyline polyline = new FingerPaintPolyline
                            {
                                StrokeColor = strokeColor,
                                StrokeWidth = strokeWidth
                            };
                            polyline.Path.MoveTo(ConvertToPixel(args.Location));

                            inProgressPolylines.Add(args.Id, polyline);
                            canvasView.InvalidateSurface();
                        }
                    }
                    break;

                case TouchActionType.Moved:
                    if (inProgressPolylines.ContainsKey(args.Id))
                    {
                        FingerPaintPolyline polyline = inProgressPolylines[args.Id];
                        polyline.Path.LineTo(ConvertToPixel(args.Location));
                        canvasView.InvalidateSurface();
                    }
                    break;

                case TouchActionType.Released:
                case TouchActionType.Exited:
                    if (inProgressPolylines.ContainsKey(args.Id))
                    {
                        completedPolylines.Add(inProgressPolylines[args.Id]);
                        inProgressPolylines.Remove(args.Id);
                        canvasView.InvalidateSurface();
                    }
                    break;

                case TouchActionType.Cancelled:
                    if (inProgressPolylines.ContainsKey(args.Id))
                    {
                        inProgressPolylines.Remove(args.Id);
                        canvasView.InvalidateSurface();
                    }
                    break;
            }
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKCanvas canvas = args.Surface.Canvas;
            canvas.Clear();

            foreach (FingerPaintPolyline polyline in completedPolylines)
            {
                paint.Color = polyline.StrokeColor.ToSKColor();
                paint.StrokeWidth = polyline.StrokeWidth;
                canvas.DrawPath(polyline.Path, paint);
            }

            foreach (FingerPaintPolyline polyline in inProgressPolylines.Values)
            {
                paint.Color = polyline.StrokeColor.ToSKColor();
                paint.StrokeWidth = polyline.StrokeWidth;
                canvas.DrawPath(polyline.Path, paint);
            }
        }

        SKPoint ConvertToPixel(Point pt)
        {
            return new SKPoint((float)(canvasView.CanvasSize.Width * pt.X / canvasView.Width),
                               (float)(canvasView.CanvasSize.Height * pt.Y / canvasView.Height));
        }

        float ConvertToPixel(float fl)
        {
            return (float)(canvasView.CanvasSize.Width * fl / canvasView.Width);
        }
    }
}