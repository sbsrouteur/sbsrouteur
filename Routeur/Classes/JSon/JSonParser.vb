Imports System.Text

Module JSonParser
    Private ObjectNum As Integer = 0

    Public Function Parse(ByVal Data As String) As Dictionary(Of String, Object)

        Dim RetValue As New Dictionary(Of String, Object)
        Dim CurIndex As Integer = 0
        Dim NextIndex As Integer = 0
        Dim ObjectData As Object = Nothing
        ObjectNum = 0

        If Data.Trim = "null" Then
            Return RetValue
        End If

        Dim CurDataString = Data.Substring(CurIndex)

        If Data(0) = "{"c Then
            'Start a new object
            Dim ObjectName As String = ""
            NextIndex = ReadObject(CurDataString, ObjectData) + CurIndex
            RetValue.Add(JSONDATA_BASE_OBJECT_NAME, ObjectData)

        Else
            Throw New InvalidOperationException("invalid json data")
        End If

        Return RetValue

    End Function

    Private Function ReadArray(ByVal Data As String, ByVal Arr As List(Of Object)) As Integer

        Dim Complete As Boolean = False
        Dim Index As Integer = 1
        Dim RetIndex As Integer = 1
        Dim CurData As String
        CurData = Data.Substring(1)
        Do
            Dim ValueObj As Object = Nothing
            Index = ReadValue(CurData, ValueObj)
            RetIndex += Index
            Arr.Add(ValueObj)
            If CurData(Index) = "," Then
                RetIndex += 1
                Index += 1
            ElseIf CurData(Index) = "]"c Then
                Complete = True
                RetIndex += 1
            Else
                Throw New InvalidOperationException("invalid json data")
            End If
            CurData = CurData.Substring(Index)

        Loop Until Complete

        Return RetIndex

    End Function

    Private Function ReadDouble(ByVal Data As String, ByRef dblValue As Double) As Integer
        Dim SB As New StringBuilder
        Dim Complete As Boolean = False
        Dim CurIndex As Integer = 0

        SB.Length = 0
        While Not Complete
            Select Case Data(CurIndex)
                Case ","c, "]"c, "}"c
                    Complete = True

                Case Else
                    'Read
                    SB.Append(Data(CurIndex))
                    CurIndex += 1
            End Select
        End While

        Double.TryParse(SB.ToString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, dblValue)

        Return CurIndex

    End Function

    Private Function ReadObject(ByVal Data As String, ByRef RetObj As Object) As Integer

        Dim CurData As String = Data
        Dim Index As Integer = 1
        Dim CurIndex As Integer = 1
        Dim ObjectComplete As Boolean
        Dim Name As String = ""
        Dim ObjData As Object = Nothing
        RetObj = New Dictionary(Of String, Object)

        If Data(0) <> "{"c Then
            Throw New InvalidOperationException("Unsupported JSon Object string")
        End If

        While Not ObjectComplete
            If CurData(Index) <> """"c Then
                Throw New InvalidOperationException("Unsupported JSon Object string")
            End If

            CurData = CurData.Substring(Index)
            Index = ReadString(CurData, Name)
            CurIndex += Index
            If CurData(Index) <> ":"c Then
                Throw New InvalidOperationException("Unsupported JSon Object string")
            End If

            CurData = CurData.Substring(Index + 1)
            Index = ReadValue(CurData, ObjData)
            CurIndex += Index + 1
            CType(RetObj, Dictionary(Of String, Object)).Add(Name, ObjData)

            If CurData(Index) = "}"c Then
                ObjectComplete = True
                CurIndex += 1
            ElseIf CurData(Index) = ","c Then
                Index += 1
                CurIndex += 1
            Else
                Throw New InvalidOperationException("Unsupported JSon Object string")
            End If

        End While


        Return CurIndex

    End Function

    Private Function ReadString(ByVal Data As String, ByRef StringValue As String) As Integer
        Dim SB As New StringBuilder
        Dim Complete As Boolean = False
        Dim CurIndex As Integer = 1

        SB.Length = 0
        While Not Complete
            Select Case Data(CurIndex)
                Case """"c
                    Complete = True
                    CurIndex += 1
                Case "\"c
                    'Read new char
                    Select Case Data(CurIndex + 1)
                        Case """"c, "\"c, "/"c
                            SB.Append(Data(CurIndex + 1))
                        Case "b"c
                            SB.Append(vbBack)
                        Case "f"c
                            SB.Append(vbFormFeed)
                        Case "n"c
                            SB.Append(vbNewLine)
                        Case "r"c
                            SB.Append(vbCrLf)
                        Case "t"c
                            SB.Append(vbTab)
                        Case "u"c
                            SB.Append(ChrW(CInt("&h" & Data.Substring(CurIndex + 2, 4))))
                        Case Else
                            Throw New InvalidOperationException("Unsupported JSon string")
                    End Select

                    If Data(CurIndex + 1) = "u"c Then
                        CurIndex += 6
                    Else
                        CurIndex += 2
                    End If
                Case Else
                    SB.Append(Data(CurIndex))
                    CurIndex += 1
            End Select
        End While

        StringValue = SB.ToString

        Return CurIndex

    End Function

    Private Function ReadValue(ByVal Data As String, ByRef Value As Object) As Integer

        Dim Index As Integer = 0

        Select Case Data(Index)
            Case "{"c
                Index = ReadObject(Data, Value)
            Case """"c
                Dim str As String = ""
                Index = ReadString(Data, str)
                Value = str
            Case "["c
                Dim Arr As New List(Of Object)
                Index = ReadArray(Data, Arr)
                Value = Arr

            Case "0"c To "9"c, "-"c
                Dim v As Double
                Index = ReadDouble(Data, v)
                Value = v

            Case "]"c
                Value = Nothing
                Index = 0

            Case "n"c
                If Data.Substring(0, 4) = "null" Then
                    Value = Nothing
                    Index = 4
                Else
                    Throw New InvalidOperationException("Unsupported JSon Value type")

                End If

            Case "f"c, "t"c
                If Data.Substring(0, 4) = "true" Then
                    Value = True
                    Index = 4
                ElseIf Data.Substring(0, 5) = "false" Then
                    Value = False
                    Index = 5
                Else
                    Throw New InvalidOperationException("Unsupported JSon Value type")
                End If



            Case Else
                Throw New InvalidOperationException("Unsupported JSon Value type")
        End Select

        Return Index

    End Function
End Module
