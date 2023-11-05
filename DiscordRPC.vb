'Option Strict On
Imports System.Dynamic
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Security.Policy
Imports System.ServiceModel.Security
Imports System.Text
Imports DiscordRPC
Imports MessagePack
Imports MessagePack.Resolvers
Imports Microsoft.SqlServer.Server
Imports DiscordRPC.Logging

Public Class DiscordRPC
    Private Shared Discord As DiscordRpcClient

    '/umamusume/load/index userdata
    Private Shared mainUserData As Object = Nothing


    <DllExport(CallingConvention.Cdecl)>
    Public Shared Function init() As Boolean
        Discord = New DiscordRpcClient("838861689294028904")
        If Not Discord.IsInitialized Then
            Discord.Initialize()
            Discord.RegisterUriScheme()
            Discord.SetSubscription(EventType.Join Or EventType.JoinRequest)

            '초기상태로 설정
            Discord.SetPresence(New RichPresence() With {
                    .State = "우마무스메",
                    .Details = "지금 막 실행",
                    .Assets = New Assets() With {
                    .LargeImageKey = "uma-logo",
                    .LargeImageText = "ウマ娘 プリティーダービー",
                    .SmallImageKey = "image_small"}
             })
            Return True
        End If
        Return False
    End Function

    <DllExport(CallingConvention.Cdecl)>
    Public Shared Sub initDB(ByVal dbpath As String)
        Helper.Log("init db", LogLevel.Info)
        UmaResources.initDB(dbpath)

    End Sub

    <DllExport(CallingConvention.Cdecl)>
    Public Shared Sub releaseDB()
        Helper.Log("db Release", LogLevel.Info)
        UmaResources.releaseDB()
    End Sub

    <DllExport(CallingConvention.Cdecl)>
    Public Shared Sub disposeRPC()
        Discord.ClearPresence()
        Discord.Dispose()
    End Sub
    <DllExport(CallingConvention.Cdecl)>
    Public Shared Sub processRPC(ByVal msgPackData As IntPtr, ByVal dataSize As Integer, ByVal rawUrl As String)
        'pointer -> array conversion
        Dim msgPack As Byte() = New Byte(dataSize - 1) {}
        Marshal.Copy(msgPackData, msgPack, 0, dataSize)

        'parse URL
        Dim url As Uri = New Uri(rawUrl)

        'Debug
        Helper.Log($"sz={dataSize}, URL={url.LocalPath}", LogLevel.Info)

        'deserialize msgpack data

        Try
            Dim data As Object

            Try
                data = MessagePackSerializer.Deserialize(Of Object)(msgPack, ContractlessStandardResolver.Options)
            Catch ex As MessagePackSerializationException
                '역직렬화에 실패하는 경우가 있는데, 이유를 모르겠음
                '멍청하게 json으로 변환했다 다시 msgpack으로 변환하고 다시 역직렬화하면 됨
                '나도 엄청 비효울적인거 아는데. 근데 에러 나는 이유를 모르겠는데.

                Dim json = MessagePackSerializer.ConvertToJson(msgPack)
                Dim tempMsgP = MessagePackSerializer.ConvertFromJson(json)

                data = MessagePackSerializer.Deserialize(Of Object)(tempMsgP, ContractlessStandardResolver.Options)
            End Try

            'process RPC
            Select Case url.LocalPath
            '초기 유저데이터 로딩
                Case "/umamusume/load/index"
                    'Console.WriteLine("Index")
                    mainUserData = data
                    Helper.Log($"set mainUserData {mainUserData.GetType()}", LogLevel.Info)

                    Discord.SetPresence(New RichPresence() With {
                            .State = "메인 화면",
                            .Details = $"Trainer {Helper.FulltoHalfWidthKana(data("data")("user_info")("name"))}",
                            .Timestamps = Timestamps.Now,
                            .Assets = New Assets() With {
                                .LargeImageKey = "uma-logo",
                                .LargeImageText = "ウマ娘 プリティーダービー",
                                .SmallImageKey = "image_small"
                            }
                     })
                '주크박스 렌덤
                Case "/umamusume/jukebox/draw_random_request"
                    Dim songName As String = Helper.FulltoHalfWidthKana(UmaResources.getText(16, data("data")("request_history")("music_id"), True))

                    Discord.SetPresence(New RichPresence() With {
                            .State = "메인 화면",
                            .Details = $"♪{songName}",
                            .Timestamps = Timestamps.Now,
                            .Assets = New Assets() With {
                                .LargeImageKey = "uma-logo",
                                .LargeImageText = "ウマ娘 プリティーダービー",
                                .SmallImageKey = "image_small"
                            }
                     })
            '라이브 선곡화면
                Case "/umamusume/live_theater/index"
                    Discord.SetPresence(New RichPresence() With {
                            .State = "라이브 시어터",
                            .Details = "선곡 중",
                            .Timestamps = Timestamps.Now,
                            .Assets = New Assets() With {
                                .LargeImageKey = "uma-logo",
                                .LargeImageText = "ウマ娘 プリティーダービー",
                                .SmallImageKey = "image_small"
                            }
                     })

            '라이브 시작
                Case "/umamusume/live_theater/live_start"
                    Dim name = UmaResources.getText(16, data("data")("live_theater_save_info")("music_id"), True)
                    Dim state = String.Empty
                    Dim characount As Integer = data("data")("live_theater_save_info")("member_info_array").Length
                    If characount > 1 Then
                        Dim sb As StringBuilder = New StringBuilder()
                        sb.Append("(")
                        For i = 0 To 1
                            Dim n As String = UmaResources.getText(6, data("data")("live_theater_save_info")("member_info_array")(i)("chara_id"), True)
                            If n.Length > 6 Then
                                n = n.Truncate(6, "...")
                            End If
                            sb.Append(n)
                            If i < characount - 1 Then sb.Append(", ")
                        Next
                        sb.Append("...)")
                        state = String.Format("{0} 외 {1}명 {2}", UmaResources.getText(6, data("data")("live_theater_save_info")("member_info_array")(0)("chara_id"), True), characount - 1, sb.ToString())
                    Else
                        state = String.Format("With {0}", UmaResources.getText(6, data("data")("live_theater_save_info")("member_info_array")(0)("chara_id"), True))
                    End If
                    state = state.Substring(0, state.Length - 1)
                    Discord.SetPresence(New RichPresence() With {
                            .State = state,
                            .Details = String.Format("{0} 시청 중", name),
                            .Timestamps = Timestamps.Now,
                            .Assets = New Assets() With {
                                .LargeImageKey = "uma-logo",
                                .LargeImageText = "ウマ娘 プリティーダービー",
                                .SmallImageKey = "image_small"
                            }
                      })
                '팀대항전 첫화면
                Case "/umamusume/team_stadium/index"
                    Discord.SetPresence(New RichPresence() With
                            {
                                .State = "팀 대항전",
                                .Details = String.Format("Idle"),
                                .Timestamps = Timestamps.Now,
                                .Assets = New Assets() With
                                {
                                    .LargeImageKey = "uma-logo",
                                    .LargeImageText = "ウマ娘 プリティーダービー",
                                    .SmallImageKey = "image_small"
                                }
                            })

                '팀대항전 상대선택
                Case "/umamusume/team_stadium/opponent_list"
                    Discord.SetPresence(New RichPresence() With {
                            .State = "팀 대항전",
                            .Details = "대전 상대 선택 중",
                            .Timestamps = Timestamps.Now,
                            .Assets = New Assets() With {
                                .LargeImageKey = "uma-logo",
                                .LargeImageText = "ウマ娘 プリティーダービー",
                                .SmallImageKey = "image_small"
                            }
                      })

                '상대결정 직후
                Case "/umamusume/team_stadium/decide_frame_order"
                    Discord.SetPresence(New RichPresence() With {
                            .State = "팀 대항전",
                            .Details = $"트레이너 {Helper.FulltoHalfWidthKana(data("data")("opponent_info_copy")("user_info")("name"))}와 대전 중",
                            .Timestamps = Timestamps.Now,
                            .Assets = New Assets() With {
                                .LargeImageKey = "uma-logo",
                                .LargeImageText = "ウマ娘 プリティーダービー",
                                .SmallImageKey = "image_small"
                            }
                      })

                '결과화면
                Case "/umamusume/team_stadium/all_race_end"
                    Dim winType As Byte = data("data")("final_win_type")
                    Dim isHighscore As Boolean = data("data")("is_update_high_score")
                    Dim score As Integer = data("data")("total_score_info")("final_total_score")
                    Dim ranking As Integer = data("data")("ranking")("rank")
                    Dim classNum As Integer = data("data")("ranking")("team_class")

                    Dim retStr As String = String.Empty

                    If winType = 1 Then
                        retStr = $"승리! "
                    End If

                    retStr = retStr & $"{score}점"

                    If isHighscore Then
                        retStr = retStr & "↑,"
                    End If
                    retStr = retStr & $" {ranking}위"

                    Discord.SetPresence(New RichPresence() With {
                            .State = "팀 대항전",
                            .Details = retStr,
                            .Timestamps = Timestamps.Now,
                            .Assets = New Assets() With {
                                .LargeImageKey = "uma-logo",
                                .LargeImageText = "ウマ娘 プリティーダービー",
                                .SmallImageKey = "image_small"
                            }
                      })

                '데일리 레이스
                Case "/umamusume/daily_race/index"
                    Discord.SetPresence(New RichPresence() With {
                            .State = "데일리 레이스",
                            .Timestamps = Timestamps.Now,
                            .Assets = New Assets() With {
                                .LargeImageKey = "uma-logo",
                                .LargeImageText = "ウマ娘 プリティーダービー",
                                .SmallImageKey = "image_small"
                            }
                      })

                '육성 시나리오 선택 화면
                Case "/umamusume/pre_single_mode/index"
                    Discord.SetPresence(New RichPresence() With {
                            .State = "육성 시나리오 선택 중",
                            .Details = "Idle",
                            .Timestamps = Timestamps.Now,
                            .Assets = New Assets() With {
                                .LargeImageKey = "uma-logo",
                                .LargeImageText = "ウマ娘 プリティーダービー",
                                .SmallImageKey = "image_small"
                            }
                      })
            End Select
        Catch ex As Exception
            Helper.Log($"processRPC exception {ex}", LogLevel.Error)
            'File.WriteAllBytes("error.msgpack", msgPack)
        End Try

    End Sub


    <DllExport(CallingConvention.Cdecl)>
    Public Shared Sub setSceneID(ByVal sceneID As UmaResources.SceneId)
        'Debug
        Helper.Log($"setSceneID={sceneID}", LogLevel.Info)

        If Not ReferenceEquals(Nothing, mainUserData) Then
            Select Case sceneID
            '홈화면으로 전환했을 경우 다시 메인화면 정보 표시
                Case UmaResources.SceneId.Home
                    Discord.SetPresence(New RichPresence() With {
                            .State = "메인 화면",
                            .Details = $"Trainer {Helper.FulltoHalfWidthKana(mainUserData("data")("user_info")("name"))}",
                            .Timestamps = Timestamps.Now,
                            .Assets = New Assets() With {
                                .LargeImageKey = "uma-logo",
                                .LargeImageText = "ウマ娘 プリティーダービー",
                                .SmallImageKey = "image_small"
                            }
                     })
            End Select
        Else
            Helper.Log("mainUserData is null!", LogLevel.Warning)
        End If

    End Sub

End Class
