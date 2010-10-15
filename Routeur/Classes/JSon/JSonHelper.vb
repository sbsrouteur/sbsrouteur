﻿Imports System.Reflection

Module JSonHelper

    Public Const JSONDATA_BASE_OBJECT_NAME As String = "JSonData"


    Public Function GetJSonObjectValue(ByVal JSon As Object, ByVal ValueKey As String) As Object

        If TypeOf JSon Is Dictionary(Of String, Object) Then
            Dim D As Dictionary(Of String, Object) = CType(JSon, Dictionary(Of String, Object))

            If D.ContainsKey(ValueKey) Then
                Return D(ValueKey)

            End If
        End If

        Return Nothing

    End Function

    Public Function GetJSonStringValue(ByVal JSon As Object, ByVal ValueKey As String) As String

        Dim O As Object = GetJSonObjectValue(JSon, ValueKey)
        If O Is Nothing Then
            Return ""
        Else
            Return O.ToString
        End If

    End Function

    Public Function GetStringFromJsonObject(ByVal Json As Dictionary(Of String, Object)) As String

        Dim RetString As String = "{"
        Dim bFirst As Boolean = True
        For Each item In Json

            If bFirst Then
                bFirst = False
            Else
                RetString &= ","
            End If

            RetString &= """" & item.Key & """:"

            If item.Value Is Nothing Then

                RetString &= "null"

            ElseIf TypeOf item.Value Is Integer Then

                RetString &= CInt(item.Value).ToString(System.Globalization.CultureInfo.InvariantCulture)

            ElseIf TypeOf item.Value Is Boolean Then
                If CBool(item.Value) Then
                    RetString &= "true"
                Else
                    RetString &= "false"
                End If
            ElseIf TypeOf item.Value Is Double Then
                RetString &= CDbl(item.Value).ToString(System.Globalization.CultureInfo.InvariantCulture)
            ElseIf TypeOf item.Value Is Long Then
                RetString &= CLng(item.Value).ToString(System.Globalization.CultureInfo.InvariantCulture)
            ElseIf TypeOf item.Value Is String Then
                Dim RetStr As String = CStr(item.Value)

                For Each c As String In New String() {"""", "\", "/", vbCr, vbLf, vbCrLf, vbTab}
                    RetStr = RetStr.Replace(c, "\" & c)
                Next
                RetString &= RetStr
            ElseIf TypeOf item.Value Is Dictionary(Of String, Object) Then
                RetString &= GetStringFromJsonObject(CType(item.Value, Dictionary(Of String, Object)))
            Else
                Throw New InvalidOperationException("unsupported type for json output : " & item.Value.GetType.ToString)
            End If

        Next

        Return RetString & "}"

    End Function

    Public Function GetJSonBoolValue(ByVal JSon As Object, ByVal ValueKey As String) As Boolean

        Dim O As Object = GetJSonObjectValue(JSon, ValueKey)
        Dim tmpBool As Boolean = False
        If O Is Nothing Then
            Return False
        ElseIf TypeOf O Is Boolean Then
            Return CBool(O)
        ElseIf Boolean.TryParse(O.ToString, tmpBool) Then
            Return tmpBool
        Else
            Return False
        End If


    End Function


    Public Function GetJSonDoubleValue(ByVal JSon As Object, ByVal ValueKey As String) As Double

        Dim O As Object = GetJSonObjectValue(JSon, ValueKey)
        Dim tmpdbl As Double = 0
        If O Is Nothing Then
            Return 0
        ElseIf TypeOf O Is Double Then
            Return CDbl(O)
        ElseIf Double.TryParse(O.ToString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, tmpdbl) Then
            Return tmpdbl
        Else
            Return 0
        End If


    End Function

    Public Function GetJSonIntValue(ByVal JSon As Object, ByVal ValueKey As String) As Integer

        Dim O As Object = GetJSonObjectValue(JSon, ValueKey)
        If O Is Nothing Then
            Return 0
        ElseIf IsNumeric(O) Then
            Return CInt(O)
        Else
            Return 0
        End If

    End Function

    Public Function GetJSonLongValue(ByVal JSon As Object, ByVal ValueKey As String) As Long
        Dim O As Object = GetJSonObjectValue(JSon, ValueKey)
        If O Is Nothing Then
            Return 0
        ElseIf IsNumeric(O) Then
            Return CLng(O)
        Else
            Return 0
        End If


    End Function

    Public Sub LoadJSonDataToObject(ByVal O As Object, ByVal JSonData As Object)

        Dim Props() As PropertyInfo = O.GetType.GetProperties

        For Each P In Props

            If Not P.CanWrite Then
                Continue For
            End If

            Dim T = P.PropertyType
            Dim Value As Object = Nothing
            If T Is GetType(Integer) Then
                Value = GetJSonIntValue(JSonData, P.Name)
            ElseIf T Is GetType(Double) Then
                Value = GetJSonDoubleValue(JSonData, P.Name)
                LoadJSonDataToObject(Value, JSonData)
            ElseIf T Is GetType(Long) Then
                Value = GetJSonLongValue(JSonData, P.Name)
            ElseIf T Is GetType(Object) Then
                Value = GetJSonObjectValue(JSonData, P.Name)
                If Value Is Nothing Then
                    Continue For
                End If
                LoadJSonDataToObject(Value, JSonData)
            ElseIf T Is GetType(String) Then
                Value = GetJSonStringValue(JSonData, P.Name)
                If CStr(Value) = "" Then
                    Value = Nothing
                End If
            ElseIf T Is GetType(DateTime) Then
                Dim epoch As Integer = GetJSonIntValue(JSonData, P.Name)
                If epoch <> 0 Then
                    Dim D As DateTime = New Date(1970, 1, 1).AddSeconds(epoch)
                    D = CDate(D).AddHours(System.TimeZone.CurrentTimeZone.GetUtcOffset(D).TotalHours)
                    Value = D
                Else
                    Value = Nothing
                End If
            ElseIf T Is GetType(List(Of VLM_RaceWaypoint)) Then
                Dim WPL As Dictionary(Of String, Object) = CType(GetJSonObjectValue(JSonData, P.Name), Dictionary(Of String, Object))

                If WPL IsNot Nothing Then
                    Dim L As List(Of VLM_RaceWaypoint) = CType(P.GetValue(O, Nothing), List(Of VLM_RaceWaypoint))
                    Dim RaceWayPoint As VLM_RaceWaypoint

                    For Each Item In WPL.Values
                        RaceWayPoint = New VLM_RaceWaypoint
                        LoadJSonDataToObject(RaceWayPoint, Item)
                        L.Add(RaceWayPoint)
                    Next

                End If


            Else
                Continue For
            End If
            If Not Value Is Nothing Then
                P.SetValue(O, Value, Nothing)
            End If

        Next

    End Sub

End Module
