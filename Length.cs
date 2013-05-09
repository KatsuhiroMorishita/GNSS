/**************************************************************************
 * Lenght.cs
 * 距離を表すための構造体 for C#
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
 *           2012/8/14  ToString()を追加した。
 * ***********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GNSS
{
    /// <summary>
    /// 南北、東西方向の距離・長さを収めるための構造体
    /// </summary>
    [Serializable]
    public struct Length
    {
        /// <summary>東西方向の長さ</summary>
        public double E;
        /// <summary>南北方向の長さ</summary>
        public double N;
        /// <summary>
        /// 文字列化して返す
        /// <para>デリミタにはカンマを使います。</para>
        /// <para>格納は、経度方向,緯度方向の順です。</para>
        /// </summary>
        /// <returns>カンマ区切りで文字列化した情報</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(100);
            sb.Append(this.E.ToString("0.0000")).Append(",").Append(this.N.ToString("0.0000"));
            return sb.ToString();
        }
        /// <summary>
        /// 構造体初期化
        /// </summary>
        /// <param name="_E">経度方向の長さ</param>
        /// <param name="_N">緯度方向の長さ</param>
        public Length(double _E = 0.0d, double _N = 0.0d) { this.E = _E; this.N = _N; }
    }
}
