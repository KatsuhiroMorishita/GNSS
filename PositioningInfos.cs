/**************************************************************************
 * PositioningInfos.cs
 * 時系列の測位情報クラス for C#
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
 *           -----------------------------------------------------------------------------
 *           2012/5/6   測位情報を配列で確保していたのをListを使ったオブジェクトへ変更した。
 *                      測位情報を追加するAddを追加した。
 *                      [Serializable]を解除
 *                      データ数を表すLengthを追加
 *           2012/5/12  RectangleFieldクラスの更新に伴いGetRectangleField()を更新した。
 *                      動作は未確認だし、実はRectangleFieldクラスは現時点では完成していない。
 *           2012/8/10  NMEAクラス以外からも利用できるように、所属名前空間を変えた。
 *                      ToString()の実装を書き換えた。
 *           2012/8/11  GetData()を整備した。　
 * ***********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GNSS.Field;

namespace GNSS
{
    /// <summary>
    /// 時系列の測位情報クラス
    /// </summary>
    public class PositioningInfos
    {
        /*-------メンバ変数----------------------------*/
        /// <summary>
        /// 測位情報
        /// </summary>
        private List<PositioningInfo> data = new List<PositioningInfo>(0);
        /*-------プロパティ----------------------------*/
        /// <summary>
        /// 保持しているデータ数
        /// </summary>
        public int Length
        {
            get {
                return this.data.Count;
            }
        }
        /*-------メソッド------------------------------*/
        /// <summary>
        /// 測位情報を追加する
        /// </summary>
        /// <param name="posInfo">測位情報</param>
        public void Add(PositioningInfo posInfo)
        {
            this.data.Add(posInfo);
            return;
        }
        /// <summary>
        /// 測位座標情報をコピーする
        /// </summary>
        /// <returns>測位座標情報</returns>
        public Blh[] GetPositions()
        {
            Blh[] ans = new Blh[data.Count];

            for (int i = 0; i < data.Count; i++) ans[i] = data[i].Position;
            return ans;
        }
        /// <summary>
        /// 時刻情報をコピーしてくれる専用メソッド
        /// </summary>
        /// <returns>時刻情報</returns>
        public DateTime[] GetTimes()
        {
            DateTime[] ans = new DateTime[data.Count];

            for (int i = 0; i < data.Count; i++) ans[i] = data[i].Time;
            return ans;
        }
        /// <summary>
        /// 観測情報を配列に加工して返す
        /// </summary>
        /// <returns>観測情報</returns>
        public PositioningInfo[] GetData()
        {
            return this.data.ToArray();
        }
        /// <summary>
        /// データ内容をstring型にして返す
        /// データには、時刻・経度・緯度・高度が含まれます。
        /// </summary>
        /// <returns>文字列化した測位情報履歴</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(3375000);                      // 予め大きなメモリ容量を確保しておく

            for (int i = 0; i < this.data.Count; i++)
            {
                sb.Append(this.data[i].ToString()).Append(System.Environment.NewLine);
            }   
            return sb.ToString();
        }
        /// <summary>
        /// 指定ファイル名でデータを保存する
        /// KMLに加工するメソッドもその内作りたいなぁ。
        /// </summary>
        /// <param name="fname">ファイル名</param>
        public void SaveFileAsNormal(string fname)
        {
            using (System.IO.StreamWriter nmea_writer = new System.IO.StreamWriter(fname))
            {
                nmea_writer.Write(this.ToString());
            }
            return;
        }
        /* [2012/5/12] 更新後は動作未確認 */
        /// <summary>
        /// ログを全走査し、2つ以上のログが存在した場合に、ログを覆う長方形領域を返す
        /// </summary>
        /// <returns>
        /// 領域情報またはnull
        /// <para>領域を構成できない場合はnullを返します。</para>
        /// </returns>
        public RectangleField GetRectangleField()
        {
            RectangleField field = new RectangleField();

            if (this.data.Count >= 2)                       // 領域は2点以上なければ指定できない
            {
                foreach (PositioningInfo pos in this.data)
                {
                    field.Set(pos.Position);
                }
                return field;
            }
            else
            {
                return null;
            }
        }
        /*-------コンストラクタ系----------*/
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PositioningInfos() { }
        /// <summary>
        /// デスコンストラクタ
        /// </summary>
        ~PositioningInfos() { }
    }
}
