Imports DiscordRPC.Logging

Public Class UmaResources
    Private Shared MainDB As SQLite3 = Nothing

    ''' <summary>
    ''' DB texts 테이블
    ''' 성능 향상을 위해 메모리에 미리 로드
    ''' </summary>
    Public Shared texts As DataTable = Nothing

    ''' <summary>
    ''' Gallop Scene ID 열거형
    ''' </summary>
    Public Enum SceneId
        None
        Title
        Home
        Race
        Live
        Story
        Gacha
        Episode
        SingleMode
        OutGame
        LiveTheater
        Circle
        DailyRace
        LegendRace
        TeamStadium
        CraneGame
        Champions
        ChampionsLobby
        Tutorial
        StoryEvent
        ChallengeMatch
        RoomMatch
        PracticeRace
        TrainingChallenge
        TeamBuilding
        FanRaid
        CampaignRaffle
        StoryMovie
        CollectEventMap
        CollectRaid
        MapEvent
        FactorResearch
        Heroes
        Max
    End Enum


    ''' <summary>
    ''' 메인 DB로드
    ''' </summary>
    ''' <param name="dbPath">DB파일 경로</param>
    ''' <returns></returns>
    Public Shared Function initDB(ByVal dbPath As String) As Boolean

        Try
            MainDB = New SQLite3(dbPath)
            Dim textCount = MainDB.ReadSQL("select * from text_data")
            texts = MainDB.Table.Copy()

            'Debug
            Helper.Log($"DBLoad OK {MainDB.DBPath}", LogLevel.Info)
            Helper.Log($"text_data Load OK {textCount}", LogLevel.Info)
        Catch ex As Exception

            Helper.Log($"DBLoad error {ex}", LogLevel.Error)
            Return False
        End Try

        Return True
    End Function

    Public Shared Function releaseDB() As Boolean
        Try
            MainDB.Close()
            Helper.Log($"DBClose OK {MainDB.DBPath}", LogLevel.Info)
            Return True
        Catch ex As Exception
            Helper.Log($"DBClose error {ex}", LogLevel.Error)
            Return False
        End Try

    End Function

    ''' <summary>
    ''' text_data 텍스트값 가져오기
    ''' </summary>
    ''' <param name="id">카테고리 ID</param>
    ''' <param name="index">실제 텍스트 값 ID</param>
    ''' <param name="isHalfKana">반각 가타카나로 출력</param>
    ''' <returns></returns>
    Public Shared Function getText(ByVal id As Integer, ByVal index As Integer, ByVal isHalfKana As Boolean) As String
        For Each row As DataRow In texts.Rows
            If Convert.ToInt32(row("id")) = id AndAlso Convert.ToInt32(row("index")) = index Then
                Return If(isHalfKana, Helper.FulltoHalfWidthKana(row("text").ToString()), row("text").ToString())
            End If
        Next
        Return Nothing
    End Function

End Class
