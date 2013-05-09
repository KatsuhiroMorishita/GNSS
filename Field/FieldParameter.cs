using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Geodesy.GeodeticDatum;

namespace GNSS.Field
{
    /// <summary>
    /// 長方形領域を表すRectangleFieldクラスのパラメータ
    /// </summary>
    internal struct FieldParameter
    {
        /** パラメータ ********************************************************************/
        /// <summary>
        /// 北限の緯度
        /// <para>未定義ではNaNを取ります。</para>
        /// </summary>
        public double upperLat;
        /// <summary>
        /// 南限の緯度
        /// <para>未定義ではNaNを取ります。</para>
        /// </summary>
        public double lowerLat;
        /// <summary>
        /// 東端の経度
        /// <para>未定義ではNaNを取ります。</para>
        /// </summary>
        public double eastLon;
        /// <summary>
        /// 西端の経度
        /// <para>未定義ではNaNを取ります。</para>
        /// </summary>
        public double westLon;
        /// <summary>
        /// 中央の経度
        /// <para>未定義ではNaNを取ります。</para>
        /// </summary>
        public double centerLon;
        /// <summary>
        /// 緯度・経度の単位
        /// </summary>
        public readonly AngleUnit unit;
        /// <summary>
        /// 測地系
        /// </summary>
        public readonly Datum datum;
        /** プロパティ ********************************************************************/
        /// <summary>
        /// 面積が存在するかを返す
        /// <para>true: 面積は0。</para>
        /// </summary>
        public Boolean AreaIsZero
        {
            get
            {
                // 座標が同じだったり、非値であれば面積0とみなす
                if (this.upperLat == this.lowerLat || this.eastLon == this.westLon || (double.IsNaN(this.upperLat) || double.IsNaN(this.lowerLat) || double.IsNaN(this.eastLon) || double.IsNaN(this.westLon)))
                    return true;
                else
                    return false;
            }
        }
        /** メソッド ********************************************************************/
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="unit">単位(°or rad)</param>
        /// <param name="datum">測地系</param>
        public FieldParameter(AngleUnit unit = AngleUnit.Degree, Datum datum = Datum.WGS84)
        {
            this.unit = unit;
            this.datum = datum;
            this.upperLat = double.NaN;
            this.eastLon = double.NaN;
            this.lowerLat = double.NaN;
            this.westLon = double.NaN;
            this.centerLon = double.NaN;
        }
    }
}
