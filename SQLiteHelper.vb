Imports System
Imports System.Collections.Generic
Imports System.Data
Imports Microsoft.Data.Sqlite

''' <summary>
''' SQLite DB 파일 도우미
''' </summary>
Public Class SQLite3
    Private _Table As System.Data.DataTable, _DBPath As String

    Public Property Table As DataTable
        Get
            Return _Table
        End Get
        Private Set(ByVal value As DataTable)
            _Table = value
        End Set
    End Property

    Public Property DBPath As String
        Get
            Return _DBPath
        End Get
        Private Set(ByVal value As String)
            _DBPath = value
        End Set
    End Property
    Private conn As SqliteConnection = Nothing
    Private trans As SqliteTransaction = Nothing
    Private parameters As List(Of SqliteParameter) = New List(Of SqliteParameter)()
    'private static ILog log = null;
    ''' <summary>
    ''' SQLiteDBTool 초기화
    ''' </summary>
    ''' <param name="dbpath">SQLite DB 파일 경로</param>
    Public Sub New(ByVal dbpath As String)
        'log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        conn = New SqliteConnection($"Data Source={dbpath}")
        Me.DBPath = dbpath
        conn.Open()
        'conn.
        Table = New DataTable()
    End Sub
    ''' <summary>
    ''' SQL 쿼리문 실행.
    ''' 쿼리가 실행되면 설정된 모든 바인드 함수는 제거됩니다.
    ''' </summary>
    ''' <param name="sqlstr">SQL 쿼리 문자열</param>
    ''' <returns>결과 행 수</returns>
    Public Function ReadSQL(ByVal sqlstr As String) As Integer
        Try
            For Each param In parameters
                'Console.WriteLine(string.Format("{0}({2})=[{1}]", param.ParameterName, param.Value, param.Value.GetType()));
                'Console.WriteLine("{0}({2})=[{1}]", param.ParameterName, param.Value, param.Value.GetType());
            Next
            'Console.WriteLine(sqlstr);
            If sqlstr.ToLower().StartsWith("select") Then
                Using cmd As SqliteCommand = New SqliteCommand(sqlstr, conn)

                    'cmd.CommandType = CommandType.TableDirect;
                    cmd.Parameters.AddRange(parameters)

                    Using rdr As SqliteDataReader = cmd.ExecuteReader()
                        Try
                            Table = New DataTable()

                            Table.Load(rdr, LoadOption.Upsert)

                            parameters.Clear() '끝나면 클리어
                        Catch __unusedConstraintException1__ As ConstraintException
                            Console.WriteLine(String.Format("null value detected!"))
                            'Console.WriteLine("null value detected!");
                            parameters.Clear()
                        End Try
                        Return Table.Rows.Count
                    End Using
                End Using
            Else
                Using insertCommand As SqliteCommand = conn.CreateCommand()
                    insertCommand.CommandText = sqlstr
                    insertCommand.Parameters.AddRange(parameters)
                    '  insertCommand.ExecuteReader();
                    Return insertCommand.ExecuteReader().RecordsAffected
                End Using


            End If
        Catch ex As Exception
            Console.WriteLine("Error while excuting SQL {0}", ex)
            Return -1
        End Try

    End Function
    ''' <summary>
    ''' Bind 함수 설정
    ''' </summary>
    ''' <param name="bindname">Bind 함수 이름</param>
    ''' <param name="data">들어갈 데이터</param>
    Public Sub Bind(ByVal bindname As String, ByVal data As Object)
        For i = 0 To parameters.Count - 1
            If parameters(i).ParameterName Is bindname Then
                parameters(i).Value = data
                Return
            End If
        Next
        parameters.Add(New SqliteParameter(bindname, data))
    End Sub
    ''' <summary>
    ''' 현재 설정된 바인드 함수 값을 반환
    ''' </summary>
    ''' <param name="bindname">바인드 함수 이름</param>
    ''' <returns>해당하는 바인드 함수가 있으면 그 함수의 값, 없으면 null</returns>
    Public Function GetBind(ByVal bindname As String) As Object
        For i = 0 To parameters.Count - 1
            If parameters(i).ParameterName Is bindname Then
                Return parameters(i).Value
            End If
        Next
        Return Nothing
    End Function
    ''' <summary>
    ''' 설정되어 있는 모든 바인드 함수 클리어(제거)
    ''' </summary>
    Public Sub BindClear()
        parameters.Clear()
    End Sub
    ''' <summary>
    ''' 특정 바인드 함수 클리어
    ''' </summary>
    ''' <param name="bindname">바인드 함수 이름</param>
    ''' <returns>클리어에 성공하면 true, 없거나 실패하면 false</returns>
    Public Function BindClear(ByVal bindname As String) As Boolean
        For i = 0 To parameters.Count - 1
            If parameters(i).ParameterName Is bindname Then
                Return parameters.Remove(parameters(i))
            End If
        Next
        Return False
    End Function
    ''' <summary>
    ''' 다른 DB파일 Attach
    ''' </summary>
    ''' <param name="dbpath">Attach 할 DB 파일 경로</param>
    ''' <param name="name">Attach 할 DB 별칭</param>
    Public Sub Attach(ByVal dbpath As String, ByVal name As String)
        Dim sqlstr = String.Format("attach ""{0}"" as '{1}'", dbpath, name)
        Using cmd As SqliteCommand = New SqliteCommand(sqlstr, conn)
            Console.WriteLine(cmd.CommandText)
            cmd.ExecuteReader()
        End Using
    End Sub

    ''' <summary>
    ''' 부착된 DB를 Detach
    ''' </summary>
    ''' <param name="name">Detach 할 DB 별칭</param>
    Public Sub Detach(ByVal name As String)
        Dim sqlstr = String.Format("detach '{0}'", name)
        Using cmd As SqliteCommand = New SqliteCommand(sqlstr, conn)
            Console.WriteLine(cmd.CommandText)
            cmd.ExecuteReader()
        End Using

    End Sub

    Public Sub BeginTrans()
        trans = conn.BeginTransaction()
    End Sub

    Public Sub Commit()
        trans.Commit()
    End Sub

    Public Sub Rollback()
        trans.Rollback()
    End Sub

    Public Sub Close()
        conn.Close()
    End Sub
End Class
