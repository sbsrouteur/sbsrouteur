Module JSonHelper

    Public Function GetJasonStringValue(ByVal JSon As Object, ByVal ValueKey As String) As String

        If TypeOf JSon Is Dictionary(Of String, Object) Then
            Dim D As Dictionary(Of String, Object) = CType(JSon, Dictionary(Of String, Object))

            If D.ContainsKey(ValueKey) Then
                If TypeOf D(ValueKey) Is String Then
                    Return CStr(D(ValueKey))
                Else
                    Return ""
                End If
            Else
                Return ""
            End If
        End If

        Return ""
    End Function

End Module
