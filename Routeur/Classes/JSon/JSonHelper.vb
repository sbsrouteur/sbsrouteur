Module JSonHelper

    Public Function GetJasonObjectValue(ByVal JSon As Object, ByVal ValueKey As String) As Object

        If TypeOf JSon Is Dictionary(Of String, Object) Then
            Dim D As Dictionary(Of String, Object) = CType(JSon, Dictionary(Of String, Object))

            If D.ContainsKey(ValueKey) Then
                Return D(ValueKey)

            End If
        End If

        Return Nothing

    End Function

    Public Function GetJasonStringValue(ByVal JSon As Object, ByVal ValueKey As String) As String

        Dim O As Object = GetJasonObjectValue(JSon, ValueKey)
        If O Is Nothing Then
            Return ""
        Else
            Return O.ToString
        End If

    End Function

    Public Function GetJasonIntValue(ByVal JSon As Object, ByVal ValueKey As String) As Integer

        Dim O As Object = GetJasonObjectValue(JSon, ValueKey)
        If O Is Nothing Then
            Return 0
        ElseIf IsNumeric(O) Then
            Return CInt(O)
        Else
            Return 0
        End If

    End Function

End Module
