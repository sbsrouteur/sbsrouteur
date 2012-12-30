Public Class eHashTable
    Inherits Hashtable

    
    Public Overloads Sub Add(ByVal Value As String)

        Dim H As Hashtable = Hashtable.Synchronized(Me)
        If Not H.Contains(Value) Then
            H.Add(Value, Value)
        End If
    End Sub


End Class
