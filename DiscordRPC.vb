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

        UmaResources.initDB(dbpath)

    End Sub

    <DllExport(CallingConvention.Cdecl)>
    Public Shared Sub releaseDB()
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
        Console.WriteLine($"[Uma.Helper.DiscordRPC] sz={dataSize}, URL={url.LocalPath}")

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
                    Console.WriteLine($"[Uma.Helper.DiscordRPC] set mainUserData {mainUserData.GetType()}")

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
            End Select
        Catch ex As Exception
            Console.WriteLine($"[Uma.Helper.DiscordRPC] processRPC exception {ex}")
            File.WriteAllBytes("error.msgpack", msgPack)
        End Try

    End Sub


    <DllExport(CallingConvention.Cdecl)>
    Public Shared Sub setSceneID(ByVal sceneID As UmaResources.SceneId)
        'Debug
        Console.WriteLine($"[Uma.Helper.DiscordRPC] setSceneID={sceneID}")

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
            Console.WriteLine($"[Uma.Helper.DiscordRPC] mainUserData is null!")
        End If

    End Sub

End Class
