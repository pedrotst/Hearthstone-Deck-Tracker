﻿using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace Hearthstone_Deck_Tracker
{
    /// <summary>
    /// Interaction logic for MultiProgressBar.xaml
    /// </summary>
    public partial class ManaCostBar
    {
        private readonly double[] _previousBarHeights;
        private readonly Rectangle[] _bars;
        private readonly Queue<double[]> _animationQueue;
        private bool _isAnimationRunning;
        public int AnimationDuration { get; set; }
        public int FrameDelay { get; set; }

        public ManaCostBar()
        {
            InitializeComponent();
            _previousBarHeights = new double[] {0.0, 0.0, 0.0};
            _bars = new Rectangle[] {WeaponsRect, SpellsRect, MinionsRect};
            AnimationDuration = 500;
            FrameDelay = 20;
            _animationQueue = new Queue<double[]>();
            _isAnimationRunning = false;
        }

        
        public void SetValues(double weapons, double spells, double minions, int count)
        {
            LabelCount.Content = count;

            _animationQueue.Enqueue(new double[] {ActualHeight * weapons / 100, ActualHeight * spells / 100, ActualHeight * minions / 100});

            if (!_isAnimationRunning)
                Animate();
        }

        private bool AnimateBar(Rectangle bar, double from, double to)
        {
            if (to > from)
            {
                if (bar.Height < to)
                {
                    bar.Height += (ActualHeight * FrameDelay) / AnimationDuration;
                    if (bar.Height > to)
                    {
                        bar.Height = to;
                        return true;
                    }
                }
                else
                {
                    bar.Height = to;
                    return true;
                }
            }
            else if (to < from)
            {
                if (bar.Height > to)
                {
                    var newHeight = bar.Height - (ActualHeight * FrameDelay) / AnimationDuration;
                    if (newHeight < to || newHeight < 0)
                    {
                        bar.Height = to;
                        return true;
                    }

                    bar.Height -= (ActualHeight * FrameDelay) / AnimationDuration;
                }
                else
                {
                    bar.Height = to;
                    return true;
                }
            }
            else
            {
                return true;
            }
            return false;
        }

        private async void Animate()
        {
            _isAnimationRunning = true;

            while(_animationQueue.Count > 0)
            {
                var targetValues = _animationQueue.Dequeue();

                bool[] done = {false, false, false};

                if (double.IsNaN(targetValues[0]) || targetValues[0] < 0)
                    targetValues[0] = 0.0;
                if (double.IsNaN(targetValues[1]) || targetValues[1] < 0)
                    targetValues[1] = 0.0;
                if (double.IsNaN(targetValues[2]) || targetValues[2] < 0)
                    targetValues[2] = 0.0;

                while (!done[0] || !done[1] || !done[2])
                {
                    //minions first, weapons last
                    if (!done[2])
                        done[2] = AnimateBar(_bars[2], _previousBarHeights[2], targetValues[2]);

                    if (!done[1])
                        done[1] = AnimateBar(_bars[1], _previousBarHeights[1], targetValues[1]);

                    if (!done[0])
                        done[0] = AnimateBar(_bars[0], _previousBarHeights[0], targetValues[0]);
                
                    await Task.Delay(FrameDelay);
                }

                _previousBarHeights[0] = targetValues[0];
                _previousBarHeights[1] = targetValues[1];
                _previousBarHeights[2] = targetValues[2];
            }

            _isAnimationRunning = false;
        }

    }
}