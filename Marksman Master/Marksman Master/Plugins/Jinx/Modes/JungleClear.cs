﻿#region Licensing
// ---------------------------------------------------------------------
// <copyright file="JungleClear.cs" company="EloBuddy">
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
namespace Marksman_Master.Plugins.Jinx.Modes
{
    using System.Linq;
    using EloBuddy;
    using EloBuddy.SDK;
    using Utils;

    internal class JungleClear : Jinx
    {
        public static void Execute()
        {
            var jungleMinions =
                StaticCacheProvider.GetMinions(CachedEntityType.Monsters,
                    x => x.IsValidTargetCached(Player.Instance.GetAutoAttackRange())).ToList();

            if (!jungleMinions.Any())
                return;

            if (!Settings.LaneClear.UseQInJungleClear || Player.Instance.ManaPercent < Settings.LaneClear.MinManaQ)
                return;

            if (HasMinigun && (jungleMinions.Count > 1) && !IsPreAttack &&
                (jungleMinions.Count(x => Orbwalker.GetTarget()?.DistanceCached(x) < 150) > 1))
            {
                Q.Cast();
            }
            else if (HasRocketLauncher && !IsPreAttack)
            {
                Q.Cast();
            }
        }
    }
}
