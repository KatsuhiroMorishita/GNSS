/**************************************************************************
 * NmeaReader.cs
 * NMEAデータ処理のためのクラス群 for C#
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
 *           ----------------------------------------------------------------
 *           2012/5/4   ファイル名をNMEA.csからNmeaReader.csへ変更した。
 *                      ダイアログを用いてファイルを開くOpen()をOpenWithDialog()に変更し、冗長な部分を削除した。
 *                      ストリーム読み込みの準備を行った。
 *           2012/5/7   GGAとZDAクラスの更新に合わせて若干変更した。
 *                      まだコードが冗長なのだけど、設計が固まらないので仕方なし。
 *           2012/6/6   Open(string fname)にファイルの存在を確認するコードを追加した。
 *                      ファイル名を表すfnameを追加し、コードも対応させた。
 *           2012/8/3   OpenWithDialog()がファイルを開いたかどうかを返り値で返すように変更した。
 *           2012/8/9   IsNmea()を新設した。
 *                      現時点ではGGAとZDAの検査しかしない。
 *                      GetTimes()の実装を、Listを利用したものに変更した。動作速度が上がったはず。
 * ***********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;   // for Regex

namespace GNSS.TextData.NMEA
{   
    /// <summary>
    /// NMEAパーサー
    /// <para>NMEAのログを解析して、解析結果を返します。</para>
    /// <para>
    /// 2011/7/17作りかけ
    /// 突貫で作ったので設計思想が固まっているわけでもない。
    /// 現時点では、ログを一旦全て読み込んでいるのでメモリ量がかなり必要となっている。
    /// 処理にも若干時間がかかる。
    /// 開発時間を優先した。
    /// </para>
    /// <para>
    /// 今後は、NMEAパーサが必要とされるなら、“object型でGGAクラスを返す”などの動作を実装する予定。
    /// 受け側では、以下の様にして呼び出す。
    /// <code>
    /// string nmea_str = "$GPGGA,*******";
    /// GPS.NMEA nmea_parser = new GPS.NMEA();
    /// object hoge;
    /// 
    /// hoge = nmea_parser.parse(nmea_str);         // NMEAフォーマットの文字列を処理して、オブジェクトを返してもらう。
    /// if(hoge.GetType() == tyepof(GPS.NMEA))      // オブジェクトの型をチェックし、任意の型と一致した場合に所望の処理を呼び出す。
    /// {
    ///     // たとえば、こんな感じか？
    ///     int sat = hoge.sat;
    /// }
    /// </code>
    /// NMEAパーサとして想定される使用環境
    /// 1) ログを処理する（センテンス毎・1エポック毎）
    /// 2) リアルタイムで処理する（センテンス毎に処理）
    /// </para>
    /// </summary>
    public class NmeaReader : IDisposable
    {
        /****************メンバ変数 ******************/
        /// <summary>
        /// 読み込んだテキストファイルを格納するstring変数
        /// <para>バイナリデータには非対応</para>
        /// </summary>
        private string[] text;
        /// <summary>
        /// Streamでファイルを開いているかどうかを示す
        /// <para>ファイルが開けていればture</para>
        /// </summary>
        private Boolean isOpenStatus = false;
        /// <summary>
        /// ファイル読み込みに使用するストリームオブジェクト
        /// </summary>
        private System.IO.FileStream fs;
        /// <summary>
        /// ファイルをバイナリで読み込むためのオブジェクト
        /// <para>ログのサイズが大きすぎて危険な場合はこちらでちょっとずづ読み込むようにする（予定）</para>
        /// </summary>
        private System.IO.StreamReader reader;
        /// <summary>
        /// エラーメッセージ
        /// </summary>
        private string errormsg = "";
        /// <summary>
        /// オープンしたファイル名
        /// </summary>
        private string fname = "";
        /****************プロパティ******************/
        /// <summary>
        /// バッファ上に読み込み済みならtrueとなる。
        /// 2011/7/20　時点では、streamに関しては関知しない。
        /// </summary>
        public Boolean IsOpen
        {
            get
            {
                if (this.text == null)
                    return false;
                else
                    return true;
            }
        }
        /// <summary>
        /// オープンしたファイル名
        /// </summary>
        public string FileName
        {
            get { return this.fname; }
        }
        /****************メソッド********************/
        /// <summary>
        /// ファイルを閉じる
        /// </summary>
        public void Close()
        {
            if (this.reader != null)
            {
                this.reader.Close();    // こちらを先に閉じる
                this.fs.Close();        // これが後。リソースを解放しているのか少し怪しい。
                this.fs.Dispose();
            }
            this.isOpenStatus = false;
            return;
        }
        /// <summary>
        /// リソースを開放します
        /// </summary>
        public void Dispose()
        {
            this.Close();
            return;
        }
        /// <summary>
        /// 指定されたファイルを開く
        /// <para>開いた後は、メモリ上に全展開となる。</para>
        /// <para>現時点ではデータ内にバイナリが入っているとエラーとなる。</para>
        /// </summary>
        /// <param name="fname">ファイル名</param>
        public void Open(string fname)
        {
            if (System.IO.File.Exists(fname))
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(fname);          // ファイル情報を取得する
                fi.Refresh();
                long filesize = fi.Length;                                      //ファイルのサイズを取得
                this.fname = System.IO.Path.GetFileName(fname);

                // ファイルを完全に読み込む
                using (System.IO.StreamReader sr = new System.IO.StreamReader(fname))
                {
                    List<string> txt = new List<string>(0);
                    while (sr.EndOfStream == false)
                    {
                        txt.Add(sr.ReadLine());
                    }
                    this.text = txt.ToArray();
                }
            }
            return;
        }
        /// <summary>
        /// ダイアログを用いてNMEA形式のログファイルを開く
        /// <para>ファイルのOpenに成功したかどうかを返します。</para>
        /// </summary>
        /// <returns>true: ファイルを開くことができています</returns>
        public Boolean OpenWithDialog()
        {
            OpenFileDialog f = new OpenFileDialog();

            f.Title = "NMEAフォーマットのログファイルを指定して下さい";
            f.Filter = "u-bloxログ|*.ubx|NMEAログ|*.nmea|NMEAログ|*.nme|tera termログ|*.log|テキストファイル|*.txt";
            if (f.ShowDialog() == DialogResult.OK)
            {
                this.fname = System.IO.Path.GetFileName(f.FileName);
                this.Open(f.FileName);
            }
            return this.IsOpen;
        }
        /// <summary>
        /// ストリームを使用してファイルを開く
        /// <para>巨大なログファイルを処理する際に使用する。</para>
        /// <para>ストリームを使用した処理に関しては未実装です。</para>
        /// </summary>
        /// <param name="fname"></param>
        public void OpenStream(string fname)
        {
            try
            {
                this.reader = new System.IO.StreamReader(fname);     // ファイルを開く
            }
            catch
            { }
            return;
        }
        /// <summary>
        /// 渡された文字列がNMEAセンテンスを含んでいるかを返す
        /// <para>2012/8/9時点では、GGAとZDAのみを対象とした処理を行います。</para>
        /// </summary>
        /// <param name="txt">検査文字列</param>
        /// <returns>検査結果, true: NMEAセンテンスを含んでいる</returns>
        public Boolean IsNmea(string txt)
        {
            if (GGA.CheckMatch(txt) || ZDA.CheckMatch(txt))
                return true;
            else
                return false;
        }
        /// <summary>
        /// GGAから緯度・経度を配列で取得する
        /// </summary>
        /// <returns>座標を表すBlh構造体の配列</returns>
        public Blh[] GetPositions()
        {
            PositioningInfos ans = new PositioningInfos();

            if (this.IsOpen)
            {
                for (int i = 0; i < this.text.Length; i++)
                {
                    string line = this.text[i];             // 1行分データを取得
                    string[] field = line.Split(',');       // スプリットでカンマを使って区切る
                    if (field[0] == "$GPGGA")
                    {
                        PositioningInfo pos = GGA.Parse(line);
                        ans.Add(pos);
                    }
                }
            }
            return ans.GetPositions();
        }
        /// <summary>
        /// GGAからUTCの秒単位の時刻を配列で得る
        /// <para>日付の経過などは考慮していない。</para>
        /// </summary>
        /// <returns>時刻を秒単位に変換したものの配列</returns>
        public float[] GetTimes()
        {
            List<float> times = new List<float>();
            int j = 0;
            string[] field;

            if (this.IsOpen)
            {
                for (int i = 0; i < this.text.Length; i++)
                {
                    string line = this.text[i];                         // 1行分データを取得
                    field = line.Split(',');                            // スプリットでカンマを使って区切る
                    if (field[0] == "$GPGGA")
                    {
                        int k = (int)(float.Parse(field[1]) * 100.0f);  // hhmmss.ss形式
                        times.Add((float)((k / 1000000) * 3600 + (k % 1000000) / 10000 * 60) + (float)(k % 10000) / 100.0f);    // 時分秒を秒に直す
                        j++;
                    }
                }
            }
            return times.ToArray();
        }
        /// <summary>
        /// GPGGAをトリガにして、タイミングごとにまとめた測位情報を返す
        /// <para>NMEAはセンテンスの出力順序を規定していないので、もしかすると時刻と測位位置がずれるかもしれない。</para>
        /// </summary>
        /// <returns>測位情報履歴</returns>
        public PositioningInfos GetPositioningResults()
        {
            PositioningInfos ans = new PositioningInfos();
            PositioningInfo pos = new PositioningInfo();

            if (this.IsOpen)
            {
                for (int i = 0; i < this.text.Length; i++)
                {
                    string line = this.text[i];             // 1行分データを取得
                    string[] field = line.Split(',');       // スプリットでカンマを使って区切る
                    
                    if (GGA.CheckMatch(line))
                    {
                        pos = GGA.Parse(line);
                        ans.Add(pos);
                    }
                    else if (ZDA.CheckMatch(line))
                    {
                        pos.Time = ZDA.Parse(line);
                    }
                }
            }
            return ans;
        }
        /// <summary>
        /// ZDAから時刻データ列のみを取得する
        /// GGAのトリガはないのでGetPositioningResults()とは時刻がずれる可能性がある。
        /// 日付も考慮してみる。
        /// </summary>
        /// <returns>測位情報履歴に含まれる時刻情報を配列にしたもの</returns>
        public DateTime[] GetDateTimes()
        {
            PositioningInfos ans = new PositioningInfos();

            if (this.IsOpen)
            {
                for (int i = 0; i < this.text.Length; i++)
                {
                    string line = this.text[i];             // 1行分データを取得
                    string[] field = line.Split(',');       // スプリットでカンマを使って区切る
                    if (field[0] == "$GPZDA")
                    {
                        PositioningInfo pos = new PositioningInfo();
                        DateTime time = ZDA.Parse(line);
                        pos.Time = time;
                        ans.Add(pos);
                    }
                }
            }
            return ans.GetTimes();
        }
        /****************コンストラクタ系************/
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NmeaReader()
        { }
        /// <summary>
        /// デスコンストラクタ
        /// <para>インスタンスの廃棄時に呼び出されるメソッド</para>
        /// </summary>
        ~NmeaReader()
        {
            if (this.reader != null)
            {
                this.reader.Close();                    // ファイル解放
            }
        }
    }
}
