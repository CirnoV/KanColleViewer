﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleViewer.QuestTracker.Models.Extensions;
using Grabacr07.KanColleViewer.QuestTracker.Models.Model;

namespace Grabacr07.KanColleViewer.QuestTracker.Models.Tracker
{
	/// <summary>
	/// 기종 전환 & 부대 재편 (이와이 폭전)
	/// </summary>
	internal class F27 : ITracker
	{
		private QuestProgressType lastProgress = QuestProgressType.None;
		private readonly int max_count = 2;
		private int count;

		public event EventHandler ProcessChanged;

		int ITracker.Id => 631;
		public QuestType Type => QuestType.OneTime;
		public bool IsTracking { get; set; }

		private System.EventArgs emptyEventArgs = new System.EventArgs();

		public void RegisterEvent(TrackManager manager)
		{
			manager.DestoryItemEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				var flagshipTable = new int[]
				{
					111, // 瑞鶴
					112, // 瑞鶴改
					462, // 瑞鶴改二
					467, // 瑞鶴改二甲
				};

				var fleet = KanColleClient.Current.Homeport.Organization.Fleets[1];
				if (!flagshipTable.Any(x => x == fleet?.Ships[0]?.Info.Id)) return; // 즈이카쿠 비서함

				var slotitems = fleet?.Ships[0]?.Slots;
				if (!slotitems.Any(x => x.Item.Info.Id == 153 && x.Item.Proficiency == 7)) return; // 숙련도max 영식 함전 52형丙(이와이소대)

				var homeportSlotitems = manager.slotitemTracker.SlotItems;
				count = count.Add(args.itemList.Count(x => (homeportSlotitems[x]?.Info.Id ?? 0) == 60)) // 영식함전62형(폭전)
							.Max(max_count);

				ProcessChanged?.Invoke(this, emptyEventArgs);
			};
		}

		public void ResetQuest()
		{
			count = 0;
			ProcessChanged?.Invoke(this, emptyEventArgs);
		}

		public int GetProgress()
		{
			return count * 100 / max_count;
		}

		public string ProgressText => count >= max_count ? "완료" : "즈이카쿠 비서함, 숙련도max 영식 함전 52형丙(이와이소대) 장착, 영식 함상전투기 62형(폭전) 폐기 " + count.ToString() + " / " + max_count.ToString();

		public string SerializeData()
		{
			return count.ToString();
		}

		public void DeserializeData(string data)
		{
			count = 0;
			int.TryParse(data, out count);
		}

		public void CheckOverUnder(QuestProgressType progress)
		{
			if (lastProgress == progress) return;
			lastProgress = progress;

			int cut50 = (int)Math.Ceiling(max_count * 0.5);
			int cut80 = (int)Math.Ceiling(max_count * 0.8);

			switch (progress)
			{
				case QuestProgressType.None:
					if (count >= cut50) count = cut50 - 1;
					break;
				case QuestProgressType.Progress50:
					if (count >= cut80) count = cut80 - 1;
					else if (count < cut50) count = cut50;
					break;
				case QuestProgressType.Progress80:
					if (count < cut80) count = cut80;
					break;
				case QuestProgressType.Complete:
					count = max_count;
					break;
			}
			ProcessChanged?.Invoke(this, emptyEventArgs);
		}
	}
}
