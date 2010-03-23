Imports System.Reflection

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

    Public Sub LoadJSonDataToObject(ByVal O As Object, ByVal Data As Object)

        Dim Props() As PropertyInfo = O.GetType.GetProperties

        For Each P In Props

            Dim T = P.PropertyType
            Dim Value As Object = Nothing
            If T Is GetType(Integer) Then
                Value = GetJSonIntValue(Data, P.Name)
            ElseIf T Is GetType(Object) Then
                Value = GetJSonObjectValue(Data, P.Name)
                LoadJSonDataToObject(Value, Data)
            ElseIf T Is GetType(String) Then
                Value = GetJSonStringValue(Data, P.Name)
                If CStr(Value) = "" Then
                    Value = Nothing
                End If
            ElseIf T Is GetType(DateTime) Then
                Dim epoch As Integer = GetJSonIntValue(Data, P.Name)
                If epoch <> 0 Then
                    Dim D As DateTime = New Date(1970, 1, 1).AddSeconds(epoch)
                    D = CDate(D).AddHours(System.TimeZone.CurrentTimeZone.GetUtcOffset(D).TotalHours)
                    Value = D
                Else
                    Value = Nothing
                End If
            ElseIf T Is GetType(IList()) Then
                Dim WPL = GetJSonObjectValue(Data, P.Name)

                


            Else
                Continue For
            End If
            If Not Value Is Nothing Then
                P.SetValue(O, Value, Nothing)
            End If

        Next

    End Sub

End Module
