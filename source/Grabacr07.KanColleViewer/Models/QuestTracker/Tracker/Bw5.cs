﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Models;

namespace Grabacr07.KanColleViewer.Models.QuestTracker.Tracker
{
    /// <summary>
    /// 해상호위전
    /// </summary>
    internal class Bw5 : ITracker
    {
        private readonly int max_count = 15;
        private int count;

        public event EventHandler ProcessChanged;

        int ITracker.Id => 228;
        public bool IsTracking { get; set; }

        private System.EventArgs emptyEventArgs = new System.EventArgs();

        public void RegisterEvent(TrackManager manager)
        {
            manager.BattleResultEvent += (sender, args) =>
            {
                if (!IsTracking) return;

                foreach (var ship in args.EnemyShips)
                {
                    // 13 = 잠수함
                    if (ship.Type == 13)
                        if (ship.MaxHp != int.MaxValue && ship.NowHp <= 0)
                            count += count >= max_count ? 0 : 1;
                }
                ProcessChanged?.Invoke(this, emptyEventArgs);
            };
        }

        public void ResetQuest()
        {
            count = 0;
            ProcessChanged?.Invoke(this, emptyEventArgs);
        }

        public double GetProgress()
        {
            return (double)count / max_count * 100;
        }

        public string GetProgressText()
        {
            return count >= max_count ? "완료" : $"잠수함 격침 {count} / {max_count}";
        }

        public string SerializeData()
        {
            return $"{count}";
        }

        public void DeserializeData(string data)
        {
            try
            {
                count = int.Parse(data);
            }
            catch
            {
                count = 0;
            }
        }
    }
}
