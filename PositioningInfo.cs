/**************************************************************************
 * PositioningInfo.cs
 * 測位情報を扱うクラス for C#
 * 時刻と座標をセットとして取り扱う。
 * 
 * 構造体だと初期化を必ず求められるためクラスとした。
 * 
 * 開発者　：森下功啓（K.Morhista Kumamoto-University）
 * 開発履歴：2011/7/3   GPS.cs整備開始
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
 *           2012/4/24  GPS.csのソースコードを分割して独立させた。
 *           2012/5/4   NMEA.csよりさらに分離。名前空間も変えた。
 *           --------------------------------------------------------
 *           2012/5/6   [Serializable]を解除
 *           2012/8/10  NMEAクラス以外からも利用できるように、所属名前空間を変えた。
 *                      今までNMEAの解析結果に利用を限っていたが、独自のパラメータもないしとりあえず汎用化しようと思う。
 *                      メンバ変数へアクセスさせるのをやめて、プロパティとした。
 *                      ToString()を新設した。
 * ***********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GNSS
{
    /// <summary>
    /// 観測データを取り扱うクラス
    /// <para>時刻と座標をセットとして取り扱う</para>
    /// </summary>
    public class PositioningInfo
    {
        /*-------メンバ変数-----*/
        /// <summary>
        /// 測位時刻（エポック）
        /// </summary>
        public DateTime Time
        {
            get;
            set;
        }
        /// <summary>
        /// 測位座標
        /// </summary>
        public Blh Position
        {
            get;
            set;
        }
        /*- メソッド ----------------*/
        /// <summary>
        /// データ内容をstring型にして返す
        /// <para>データには、時刻・経度・緯度・楕円体高度が含まれます。</para>
        /// <para>改行コードは含まれません。</para>
        /// </summary>
        /// <returns>文字列化した測位情報履歴</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(100);

            sb.Append(this.Time.ToString()).Append(",").Append(this.Position.ToString());
            return sb.ToString();
        }
        /*-------コンストラクタ系----------*/
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PositioningInfo() 
            :base()
        {
            
        }
        /// <summary>
        /// デスコンストラクタ
        /// </summary>
        ~PositioningInfo() { }
    }
}
