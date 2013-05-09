/**************************************************************************
 * ECEF.cs
 * ECEF座標系 for C#
 * 
 * 開発者　：森下功啓（K.Morhista Kumamoto-University）
 * 開発履歴：2011/7/3   整備開始
 *           2011/7/6   2地点間の距離を求める、GetDistanceのデバッグ。単位長までしか計算していなかった…。
 *           2011/7/17  XYZとBLHに二項演算子を追加
 *           2011/9/19  Open(string fname)のバグを修正
 *                      GetPositions(),GetPositioningResults()において、ubloxのNMEAフォーマットに対応（現時点での要求仕様を満たすだけなので万能ではないことに注意）
 *                      PositioningInfosのToString()に高度も含まれるように変更
 *           2011/11/11 よく分かっていないが、シリアル化した方が良さそうとのことで[Serializable]を片っ端からつけてみた。今後どうなるかは・・・よく分からない。
 *           2011/11/20 BLH構造体にToString()を追加
 *           2011/12/2  XML構文エラーがある部分を訂正した
 *           2011/12/15 BLH構造体にToString()を追加
 *           2012/4/22  コメントを少しだけ付け直した。
 *                      NMEAクラスは現時点ではバイナリの処理が不可能だが、そこんとこを何とかしたい。
 *           2012/4/24  ソースコードを分割した。
 *           ----------------------------------------------
 *           2012/6/5   引き算と足し算の演算を間違えていたのを修正
 *           2012/7/3   文字列で初期化するコンストラクタを整備
 *                      WGS84クラスの設計変更への対応
 *           2012/7/23  GRS80測地系へ対応
 *           2012/8/6   GlobalDatumクラスを新設したので、対応させた。
 *           2012/8/17  クラスの名称をXYZからECEFへ変更した。
 * ***********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Geodesy.GeodeticDatum;

namespace GNSS
{
    /*2011/7/6 動作確認済み*/
    /// <summary>
    /// ECEF座標系
    /// </summary>
    public struct Ecef
    {
        /************************* メンバ変数 ***************************/
        // XYZは意味的におかしな値になることがないため、メンバを公開する
        /// <summary>
        /// x座標[m]
        /// </summary>
        public double x;
        /// <summary>
        /// y座標[m]
        /// </summary>
        public double y;
        /// <summary>
        /// z座標[m]
        /// </summary>
        public double z;
        /******************** 演算子 ********************************/
        /// <summary>
        /// 二項+演算子（これで足し算が簡単にできる）
        /// </summary>
        /// <param name="c1">被加算値</param>
        /// <param name="c2">加算値</param>
        /// <returns>2値を加算した結果</returns>
        public static Ecef operator +(Ecef c1, Ecef c2)
        {
            return new Ecef(c1.x + c2.x, c1.y + c2.y, c1.z + c2.z);
        }
        /// <summary>
        /// 二項-演算子（これで足し算が簡単にできる）
        /// </summary>
        /// <param name="c1">被減算値</param>
        /// <param name="c2">減算値</param>
        /// <returns>2値の引き算の結果</returns>
        public static Ecef operator -(Ecef c1, Ecef c2)
        {
            return new Ecef(c1.x - c2.x, c1.y - c2.y, c1.z - c2.z);
        }
        /******************** プロパティ ****************************/
        /// <summary>
        /// ノルム（ベクトル長）
        /// </summary>
        public double Norm
        { 
            get {
                return (Math.Sqrt(Math.Pow(this.x, 2) + Math.Pow(this.y, 2) + Math.Pow(this.z, 2))); 
            } 
        }
        /******************** メソッド ****************************/
        /// <summary>
        /// XYZからBlh座標系（緯度・経度・楕円体高[m]）へ変換する
        /// </summary>
        /// <param name="datum">変換後の測地系</param>
        /// <returns>Blhに変換した結果</returns>
        public Blh ToBLH(Datum datum = Datum.WGS84)
        {
            Blh ans = new Blh();
            double a, b, e, n, h, p, t, sint, cost;
            GlobalDatum _datum = new GlobalDatum(datum);

            ans.H = -_datum.a;
            if (this.x == 0 && this.y == 0 && this.z == 0) return ans;

            a = _datum.a;                       // 長半径
            b = _datum.b;                       // 短半径
            e = Math.Sqrt(_datum.e2);           // 離心率

            //　座標変換のためのパラメータ
            h = Math.Pow(a, 2) - Math.Pow(b, 2);
            p = Math.Sqrt(Math.Pow(this.x, 2) + Math.Pow(this.y, 2));
            t = Math.Atan2(this.z * a, p * b);
            sint = Math.Sin(t);
            cost = Math.Cos(t);

            ans.B = Math.Atan2(this.z + h / b * Math.Pow(sint, 3), p - h / a * Math.Pow(cost, 3));  // 緯度[rad]を計算する
            ans.L = Math.Atan2(this.y, this.x);                                                     // 経度[rad]を求める
            n = a / Math.Sqrt(1 - _datum.e2 * Math.Pow(Math.Sin(ans.B), 2));                        // 卯酉線曲率半径
            ans.H = (p / Math.Cos(ans.B)) - n;                                                      // 楕円体高[m]
            return ans;
        }
        /// <summary>
        /// メンバを文字列化して返す
        /// </summary>
        /// <returns>メンバ変数x,y,zを文字列化したしたstring型変数</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(50);                      // 予め大きなメモリ容量を確保しておく

            sb.Append(this.x.ToString("0.0000")).Append(",").Append(this.y.ToString("0.0000")).Append(",").Append(this.z.ToString("0.0000"));
            return sb.ToString();
        }
        /// <summary>
        /// XYZ構造体のコンストラクタ
        /// <para>引数は省略可能です。</para>
        /// </summary>
        /// <param name="x">x成分</param>
        /// <param name="y">y成分</param>
        /// <param name="z">z成分</param>
        public Ecef(double x = 0.0d, double y = 0.0d, double z = 0.0d) 
        { 
            this.x = x; 
            this.y = y; 
            this.z = z; 
        }
        /// <summary>
        /// XYZを文字列で初期化するコンストラクタ
        /// <para>引数は省略可能です。</para>
        /// </summary>
        /// <param name="x">x成分</param>
        /// <param name="y">y成分</param>
        /// <param name="z">z成分</param>
        public Ecef(string x = "0.0", string y = "0.0", string z = "0.0")
        {
            double _x = 0.0, _y = 0.0, _z = 0.0;
            try
            {
                _x = double.Parse(x);
                _y = double.Parse(y);
                _z = double.Parse(z);

            }
            catch (Exception)
            {
                throw new Exception("XYZ構造体のコンストラクタにおいてエラーがスローされました。パラメータの文字列が解析不能です。");
            }
            this.x = _x;
            this.y = _y;
            this.z = _z;
        }
    }
}
