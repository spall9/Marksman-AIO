﻿#region Licensing
// ---------------------------------------------------------------------
// <copyright file="PermaActive.cs" company="EloBuddy">
// 
// Marksman Master
// Copyright (C) 2016 by gero
// All rights reserved
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see http://www.gnu.org/licenses/. 
// </copyright>
// <summary>
// 
// Email: geroelobuddy@gmail.com
// PayPal: geroelobuddy@gmail.com
// </summary>
// ---------------------------------------------------------------------
#endregion
using System.Linq;
using EloBuddy.SDK;
using Marksman_Master.Utils;

namespace Marksman_Master.Plugins.KogMaw.Modes
{
    internal class PermaActive : KogMaw
    {
        public static void Execute()
        {
            if (R.IsReady())
            {
                var enemy = EntityManager.Heroes.Enemies.Where(
                    x => x.IsValidTarget(R.Range) && (x.TotalHealthWithShields() - IncomingDamage.GetIncomingDamage(x) < Damage.GetRDamage(x)))
                    .OrderBy(TargetSelector.GetPriority).ThenByDescending(x=>R.GetPrediction(x).HitChancePercent).FirstOrDefault();

                if (enemy != null)
                {
                    var rPrediction = R.GetPrediction(enemy);

                    if (((HasKogMawRBuff && (GetKogMawRBuff.Count <= Settings.Combo.RAllowedStacks + 1)) || !HasKogMawRBuff) && rPrediction.HitChancePercent >= Settings.Combo.RHitChancePercent)
                    {
                        R.Cast(rPrediction.CastPosition);
                    }
                }
            }
        }
    }
}