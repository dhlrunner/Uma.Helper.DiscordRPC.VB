Imports System.Runtime.CompilerServices
Imports DiscordRPC.Logging

Public Class Helper
    Public Shared ReadOnly DictHalfFullKana As Dictionary(Of String, String) = New Dictionary(Of String, String)() From {
    {"ｱ", "ア"},
    {"ｲ", "イ"},
    {"ｳ", "ウ"},
    {"ｴ", "エ"},
    {"ｵ", "オ"},
    {"ｶ", "カ"},
    {"ｷ", "キ"},
    {"ｸ", "ク"},
    {"ｹ", "ケ"},
    {"ｺ", "コ"},
    {"ｻ", "サ"},
    {"ｼ", "シ"},
    {"ｽ", "ス"},
    {"ｾ", "セ"},
    {"ｿ", "ソ"},
    {"ﾀ", "タ"},
    {"ﾁ", "チ"},
    {"ﾂ", "ツ"},
    {"ﾃ", "テ"},
    {"ﾄ", "ト"},
    {"ﾅ", "ナ"},
    {"ﾆ", "ニ"},
    {"ﾇ", "ヌ"},
    {"ﾈ", "ネ"},
    {"ﾉ", "ノ"},
    {"ﾊ", "ハ"},
    {"ﾋ", "ヒ"},
    {"ﾌ", "フ"},
    {"ﾍ", "ヘ"},
    {"ﾎ", "ホ"},
    {"ﾏ", "マ"},
    {"ﾐ", "ミ"},
    {"ﾑ", "ム"},
    {"ﾒ", "メ"},
    {"ﾓ", "モ"},
    {"ﾔ", "ヤ"},
    {"ﾕ", "ユ"},
    {"ﾖ", "ヨ"},
    {"ﾗ", "ラ"},
    {"ﾘ", "リ"},
    {"ﾙ", "ル"},
    {"ﾚ", "レ"},
    {"ﾛ", "ロ"},
    {"ﾜ", "ワ"},
    {"ｦ", "ヲ"},
    {"ﾝ", "ン"},
    {"ｳﾞ", "ヴ"},
    {"ｶﾞ", "ガ"},
    {"ｷﾞ", "ギ"},
    {"ｸﾞ", "グ"},
    {"ｹﾞ", "ゲ"},
    {"ｺﾞ", "ゴ"},
    {"ｻﾞ", "ザ"},
    {"ｼﾞ", "ジ"},
    {"ｽﾞ", "ズ"},
    {"ｾﾞ", "ゼ"},
    {"ｿﾞ", "ゾ"},
    {"ﾀﾞ", "ダ"},
    {"ﾁﾞ", "ヂ"},
    {"ﾂﾞ", "ヅ"},
    {"ﾃﾞ", "デ"},
    {"ﾄﾞ", "ド"},
    {"ﾊﾞ", "バ"},
    {"ﾋﾞ", "ビ"},
    {"ﾌﾞ", "ブ"},
    {"ﾍﾞ", "ベ"},
    {"ﾎﾞ", "ボ"},
    {"ﾊﾟ", "パ"},
    {"ﾋﾟ", "ピ"},
    {"ﾌﾟ", "プ"},
    {"ﾍﾟ", "ペ"},
    {"ﾎﾟ", "ポ"},
    {"ｧ", "ァ"},
    {"ｨ", "ィ"},
    {"ｩ", "ゥ"},
    {"ｪ", "ェ"},
    {"ｫ", "ォ"},
    {"ｯ", "ッ"},
    {"ｬ", "ャ"},
    {"ｭ", "ュ"},
    {"ｮ", "ョ"},
    {"-", "ー"}
}

    ''' <summary>
    ''' 전각 가타카나를 반각으로 바꿔줌
    ''' </summary>
    ''' <param name="sentence">입력 문자열</param>
    ''' <returns>반각 가타카나로 변환된 문자열</returns>
    Public Shared Function FulltoHalfWidthKana(ByVal sentence As String) As String
        Dim str_Halfed As String = sentence

        For Each item In DictHalfFullKana
            Dim half As String = item.Key
            Dim full As String = item.Value
            str_Halfed = str_Halfed.Replace(full, half)
        Next

        Return str_Halfed
    End Function

    ''' <summary>
    ''' 콘솔 로깅용
    ''' </summary>
    ''' <param name="logMessage">로그 메세지</param>
    ''' <param name="logLevel">로그 레벨</param>
    Public Shared Sub Log(logMessage As String, logLevel As LogLevel)
        Select Case logLevel
            Case LogLevel.Info
                Console.ForegroundColor = ConsoleColor.Green
            Case LogLevel.Warning
                Console.ForegroundColor = ConsoleColor.Yellow
            Case LogLevel.Error
                Console.ForegroundColor = ConsoleColor.Red
        End Select

        Console.WriteLine($"[{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name} {logLevel}]: {logMessage}")

        ' 원래 콘솔 색상 복원
        Console.ResetColor()
    End Sub
End Class

Public Module StringExt
    <Extension()>
    Public Function Truncate(ByVal value As String, ByVal maxLength As Integer, ByVal Optional truncationSuffix As String = "…") As String
        Return If(value?.Length > maxLength, value.Substring(0, maxLength) & truncationSuffix, value)
    End Function
End Module
