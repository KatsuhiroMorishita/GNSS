[This is ...]
GNSS関係のライブラリです。

[IDE]                 Visual Studio 2010
[Programing Language] C#

[Abst.]
測地系はWGS84とGRS80しか考慮していません。
当面は、Blh座標系もしくはECEF座標系からENUローカル座標系へ変換するメソッドを実装することを目指します。
それが終わったら、衛星位置を処理する構造体・クラス群を整備する予定です。

[bug]
1)	NA

[memo for next works]
1) NMEAセンテンスを処理するためのクラスとして、インターフェイスクラスを作る。
2) 各センテンスの情報を収める構造体を宣言する。
3) 上の構造体を格納できるようにNMEA測位情報クラスを拡張する。
4) NmeaReaderクラスをストリーム読み込みに対応させる。
5) C++で作成する測位演算エンジンをラップ

[ref.]
1) dll import to your project, http://okachibi.web.fc2.com/dotnet/call_csdll_01.html

2013/5/11 Katsuhiro Morishita
