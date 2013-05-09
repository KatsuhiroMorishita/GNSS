/**************************************************************************
 * AngleUnit.cs
 * 角度の単位を表す列挙体 for C#
 * 
 * 開発者　：森下功啓（K.Morhista Kumamoto-University）
 * 開発履歴：
 *           2012/5/11  BLH構造体内部で宣言されていたものをここへ移した。名前もunitsからAngleUnitへ解明した。
 * ***********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GNSS
{
    /// <summary>角度の単位</summary>
    public enum AngleUnit
    {
        /// <summary>度</summary>
        Degree,
        /// <summary>ラジアン</summary>
        Radian
    }
}
