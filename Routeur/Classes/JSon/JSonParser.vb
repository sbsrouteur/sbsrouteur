'This file is part of Routeur.
'Copyright (C) 2010-2013  sbsRouteur(at)free.fr

'Routeur is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

'Routeur is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.

'You should have received a copy of the GNU General Public License
'along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

Imports System.Text

Module JSonParser
    Private ObjectNum As Integer = 0

    Public Function Parse(ByVal Data As String) As Dictionary(Of String, Object)

        Dim RetValue As New Dictionary(Of String, Object)
        Dim CurIndex As Integer = 0
        Dim NextIndex As Integer = 0
        Dim ObjectData As Object = Nothing
        ObjectNum = 0

        If Data Is Nothing OrElse Data.Trim = "null" Then
            Return RetValue
        End If

        'Dim CurDataString = Data.Substring(CurIndex)

        If Data(0) = "{"c Then
            'Start a new object
            Dim ObjectName As String = ""
            NextIndex = ReadObject(Data, CurIndex, ObjectData)
            RetValue.Add(JSONDATA_BASE_OBJECT_NAME, ObjectData)

        Else
            Throw New InvalidOperationException("invalid json data")
        End If

        Return RetValue

    End Function

    Private Function ReadArray(ByVal Data As String, ByVal startindex As Integer, ByVal Arr As List(Of Object)) As Integer

        Dim Complete As Boolean = False
        Dim Index As Integer = startindex + 1

        Do
            Dim ValueObj As Object = Nothing
            Index = ReadValue(Data, Index, ValueObj)
            Arr.Add(ValueObj)
            If Data(Index) = "," Then
                Index += 1
            ElseIf Data(Index) = "]"c Then
                Complete = True
                Index += 1
            Else
                Throw New InvalidOperationException("invalid json data")
            End If
            
        Loop Until Complete

        Return Index

    End Function

    Private Function ReadDouble(ByVal Data As String, ByVal startindex As Integer, ByRef dblValue As Double) As Integer
        Dim SB As New StringBuilder
        Dim Complete As Boolean = False
        Dim CurIndex As Integer = startindex

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

    Private Function ReadObject(ByVal Data As String, ByVal StartIndex As Integer, ByRef RetObj As Object) As Integer

        Dim Index As Integer = StartIndex
        Dim ObjectComplete As Boolean
        Dim Name As String = ""
        Dim ObjData As Object = Nothing
        RetObj = New Dictionary(Of String, Object)

        If Data(Index) <> "{"c Then
            Throw New InvalidOperationException("Unsupported JSon Object string")
        End If
        Index += 1

        While Not ObjectComplete
            If Data(Index) <> """"c Then
                Throw New InvalidOperationException("Unsupported JSon Object string")
            End If
            Index += 1
            Index = ReadString(Data, Index, Name)
            If Data(Index) <> ":"c Then
                Throw New InvalidOperationException("Unsupported JSon Object string")
            End If

            Index = ReadValue(Data, Index + 1, ObjData)
            CType(RetObj, Dictionary(Of String, Object)).Add(Name, ObjData)
            If Data(Index) = "}"c Then
                ObjectComplete = True
                Index += 1
            ElseIf Data(Index) = ","c Then
                Index += 1
            Else
                Throw New InvalidOperationException("Unsupported JSon Object string")
            End If

        End While


        Return Index

    End Function

    Private Function ReadString(ByVal Data As String, ByVal StartIndex As Integer, ByRef StringValue As String) As Integer
        Dim SB As New StringBuilder
        Dim Complete As Boolean = False
        Dim CurIndex As Integer = StartIndex

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

    Private Function ReadValue(ByVal Data As String, ByVal StartIndex As Integer, ByRef Value As Object) As Integer

        Dim Index As Integer = StartIndex

        Select Case Data(Index)
            Case "{"c
                Index = ReadObject(Data, Index, Value)
            Case """"c
                Dim str As String = ""
                Index = ReadString(Data, Index + 1, str)
                Value = str
            Case "["c
                Dim Arr As New List(Of Object)
                Index = ReadArray(Data, Index, Arr)
                Value = Arr

            Case "0"c To "9"c, "-"c
                Dim v As Double
                Index = ReadDouble(Data, Index, v)
                Value = v

            Case "]"c
                Value = Nothing
                Index = StartIndex

            Case "n"c
                If Data.Substring(StartIndex, 4) = "null" Then
                    Value = Nothing
                    Index = StartIndex + 4
                Else
                    Throw New InvalidOperationException("Unsupported JSon Value type")

                End If

            Case "f"c, "t"c
                If Data.Substring(StartIndex, 4) = "true" Then
                    Value = True
                    Index = StartIndex + 4
                ElseIf Data.Substring(StartIndex, 5) = "false" Then
                    Value = False
                    Index = StartIndex + 5
                Else
                    Throw New InvalidOperationException("Unsupported JSon Value type")
                End If



            Case Else
                Throw New InvalidOperationException("Unsupported JSon Value type")
        End Select

        Return Index

    End Function
End Module
