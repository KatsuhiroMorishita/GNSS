/**************************************************************************
 * ZDA.cs
 * NMEAのZDAセンテンスを処理するためのクラス for C#
 * 
 * 開発者　：森下功啓（K.Morhista Kumamoto-University）
 * 開発履歴：
 *          2012/5/4    整備開始
 *          2012/5/6    センテンスの解析機能を取り急ぎ実装した。アルゴリズムは腐っているけど、とりあえず間に合わせ。
 *          2012/5/7    スタティックコンストラクタを整備して、エラー対策を実装した。
 *                      正規表現を利用したセンテンス解析を実施するように改めた。
 * ***********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;   // for Regex

namespace GNSS.TextData.NMEA
{
    /// <summary>
    /// ZDAのみを扱うクラス
    /// <para>未完成</para>
    /// <para>staticクラスとして宣言するので、インスタンスの確保を宣言せずとも使えます。</para>
    /// </summary>
    public static class ZDA
    {
        /*************** メンバ変数 ************************/
        /// <summary>
        /// ZDAセンテンスにマッチさせる正規表現
        /// <para>NMEAのバージョンの違いをどうやって吸収するかはまだ未定</para>
        /// </summary>
        static private Regex regexMatch;
        /// <summary>
        /// GGAセンテンスにマッチさせて、内部の値をグループ化する正規表現
        /// <para>Pythonと違ってこちらだけでも用は足りるかもしれない。</para>
        /// </summary>
        static private Regex regexGroup;
        /*************** メソッド ************************/
        /// <summary>
        /// 文字列を検査してセンテンスにマッチするかどうかを確認する
        /// </summary>
        /// <param name="txt">被検査文字列</param>
        /// <returns>検査結果<para>true: ヒット</para></returns>
        public static Boolean CheckMatch(string txt)
        {
            Match m = regexMatch.Match(txt);
            return m.Success;
        }
        /// <summary>
        /// 検査文字列の中にあるマッチするものをすべて抽出する
        /// <para>Pythonのfindallを参考にした。</para>
        /// <para>テスト未実施</para>
        /// </summary>
        /// <param name="txt">被検査文字列</param>
        /// <returns>検査結果</returns>
        public static MatchCollection FindAll(string txt)
        {
            MatchCollection mc = regexMatch.Matches(txt);
            return mc;
        }
        /// <summary>
        /// ZDAを解析して、測位時刻を返す
        /// <para>正規表現を利用したマッチをかけているので文字列処理でエラーが生じるとは思えませんが、エラー対策をかませています。</para>
        /// </summary>
        /// <param name="str">解析文字列</param>
        /// <returns>時刻（UTC）</returns>
        public static DateTime Parse(string str)
        {
            DateTime ans = new DateTime();
            float temp;

            Match m = regexGroup.Match(str);                // マッチチェック
            if (m.Success)
            {
                string clock     = m.Groups["clock"].Value;
                string day_str   = m.Groups["day"].Value;
                string month_str = m.Groups["month"].Value;
                string year_str  = m.Groups["year"].Value;

                // 日付・時刻を取得する
                Boolean result = float.TryParse(clock, out temp);       // 時間をfloatへ変換。成功するかの確認も行う。
                if (result == false) return ans;
                int k = (int)(temp * 100.0f);                           // hhmmss.ss形式
                int hh = k / 1000000;
                int mm = (k % 1000000) / 10000;
                int ss = (k % 10000) / 100;
                result = float.TryParse(day_str, out temp);             // day_strをfloatへ変換。成功するかの確認も行う。
                if (result == false) return ans;
                int day = (int)temp;
                result = float.TryParse(month_str, out temp);           // 時間をfloatへ変換。成功するかの確認も行う。
                if (result == false) return ans;
                int month = (int)temp;
                result = float.TryParse(year_str, out temp);            // 時間をfloatへ変換。成功するかの確認も行う。
                if (result == false) return ans;
                int year = (int)temp;
                if (hh <= 24 && mm <= 59 && ss <= 59 && day <= 31 && month <= 12 && year >= 1985) ans = new DateTime(year, month, day, hh, mm, ss);
            }
            return ans;
        }
        /// <summary>
        /// スタティックコンストラクタ
        /// <para>メンバの初期化を行います。</para>
        /// </summary>
        static ZDA()
        {
            try
            {
                // 次の文字列にヒットすることは確認済み："$GPGGA,012211.00,3249.42677,N,13043.75860,E,1,08,0.99,113.3,M,29.0,M,,*5B"
                regexMatch = new Regex(@"\$GPZDA,\d+\.\d+,\d\d,\d\d,\d\d\d\d,\d\d,\d\d\*");
                // Python用のコードのままだとエラーが出たので、グループ(?P<>)の中のPを削除する必要がある。
                regexGroup = new Regex(@"\$GPZDA,(?<clock>\d+\.\d+),(?<day>\d\d),(?<month>\d\d),(?<year>\d\d\d\d),\d\d,\d\d\*");
            }
            catch 
            {
                throw new SystemException("ZDAクラスのスタティックコンストラクタにおいて、Regex（正規表現）オブジェクトの生成に失敗しました。");
            }
        }
    }
}
