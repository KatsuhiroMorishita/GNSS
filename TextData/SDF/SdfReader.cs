/**************************************************************************
 * SdfReader.cs
 * SDFフォーマットのログデータを読み込む簡易クラス
 * 
 * 開発者　：森下功啓（K.Morhista Kumamoto-University @ 2012/8）
 * 開発履歴：
 *              2012/8/9    新設
 *              2012/8/10   デバッグ完了
 *                          NmeaReaderとのインターフェイスの統一が取れていないが、現時点では放置する。
 * ***********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;   // for Regex

namespace GNSS.TextData.SDF
{
    /// <summary>
    /// SDFフォーマットのログデータを読み込む簡易クラス
    /// </summary>
    public class SdfReader
    {
        /* メンバ変数 ************************************/
        /// <summary>
        /// sdfセンテンスにマッチさせる正規表現
        /// </summary>
        private Regex regexMatch;
        /// <summary>
        /// sdfセンテンスにマッチさせて、内部の値をグループ化する正規表現
        /// </summary>
        private Regex regexGroup;
        /* プロパティ ************************************/
        /* メソッド **************************************/
        /// <summary>
        /// 文字列を解析して、そこに含まれる1エポック分の測位情報を返す
        /// </summary>
        /// <param name="line">被解析文字列</param>
        /// <returns>測位情報の解析データ<para>null: 解析不可能であった場合</para></returns>
        public PositioningInfo Parse(string line)
        {
            Match m = this.regexGroup.Match(line);                // マッチチェック
            if (m.Success)
            {
                Blh pos = new Blh();                                // 初期値は全て0になっている
                PositioningInfo ans = new PositioningInfo();

                int day = int.Parse(m.Groups["day"].Value);
                int month = int.Parse(m.Groups["month"].Value);
                int year = int.Parse(m.Groups["year"].Value);
                int hour = int.Parse(m.Groups["hour"].Value);
                int min = int.Parse(m.Groups["min"].Value);
                int sec = int.Parse(m.Groups["sec"].Value);
                ans.Time = new DateTime(year, month, day, hour, min, sec);

                pos.B = double.Parse(m.Groups["lat"].Value);
                pos.L = double.Parse(m.Groups["lon"].Value);
                pos.H = double.Parse(m.Groups["height"].Value);
                ans.Position = pos;
                return ans;
            }
            else
                return null;
        }
        /// <summary>
        /// 指定ファイルに含まれる測位情報を返す
        /// </summary>
        /// <returns>測位情報履歴</returns>
        public PositioningInfos GetPositioningResults(string fname)
        {
            PositioningInfos ans = new PositioningInfos();

            if (System.IO.File.Exists(fname))
            {
                using (System.IO.StreamReader sr = new System.IO.StreamReader(fname))
                {
                    while (sr.EndOfStream == false)             // 最後まで読み込む
                    {
                        string line = sr.ReadLine();            // 1行取得
                        var info = this.Parse(line);
                        if (info!= null)
                            ans.Add(info);
                    }
                }
            }
            return ans;
        }
        /// <summary>
        /// 渡された文字列にsdfフォーマットセンテンスが含まれるかどうかを返す
        /// </summary>
        /// <param name="txt">検査文字列</param>
        /// <returns>検査結果<para>true: sdfセンテンスです</para></returns>
        public Boolean IsSdf(string txt)
        {
            Match m = this.regexMatch.Match(txt);
            return m.Success;
        }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SdfReader()
        {
            try
            {
                // 正規表現オブジェクトを生成
                this.regexMatch = new Regex(@",\d{2}\.\d{2}\.\d{4},\d{2}:\d{2}\.\d{2},\d+\.\d+,\d+\.\d+,\d+,\d+\.?\d*,-?\d+,");
                this.regexGroup = new Regex(@",(?<day>\d{2})\.(?<month>\d{2})\.(?<year>\d{4}),(?<hour>\d{2}):(?<min>\d{2})\.(?<sec>\d{2}),(?<lat>\d+\.\d+),(?<lon>\d+\.\d+),(?<height>\d+),(?<hoge1>\d+\.?\d*),(?<hoge2>-?\d+),");
            }
            catch
            {
                throw new SystemException("SdfReaderクラスのコンストラクタにおいて、Regex（正規表現）オブジェクトの生成に失敗しました。");
            }
        }
    }
}
