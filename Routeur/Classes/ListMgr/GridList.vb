
Public Class GridList

    Implements IList(Of RoutingGridPoint)


    Public Sub Add(ByVal item As RoutingGridPoint) Implements System.Collections.Generic.ICollection(Of RoutingGridPoint).Add

    End Sub

    Public Sub Clear() Implements System.Collections.Generic.ICollection(Of RoutingGridPoint).Clear

    End Sub

    Public Function Contains(ByVal item As RoutingGridPoint) As Boolean Implements System.Collections.Generic.ICollection(Of RoutingGridPoint).Contains

    End Function

    Public Sub CopyTo(ByVal array() As RoutingGridPoint, ByVal arrayIndex As Integer) Implements System.Collections.Generic.ICollection(Of RoutingGridPoint).CopyTo

    End Sub

    Public ReadOnly Property Count() As Integer Implements System.Collections.Generic.ICollection(Of RoutingGridPoint).Count
        Get

        End Get
    End Property

    Public ReadOnly Property IsReadOnly() As Boolean Implements System.Collections.Generic.ICollection(Of RoutingGridPoint).IsReadOnly
        Get

        End Get
    End Property

    Public Function Remove(ByVal item As RoutingGridPoint) As Boolean Implements System.Collections.Generic.ICollection(Of RoutingGridPoint).Remove

    End Function

    Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of RoutingGridPoint) Implements System.Collections.Generic.IEnumerable(Of RoutingGridPoint).GetEnumerator
        Throw New NotSupportedException("GetEnumerator")
    End Function

    Public Function IndexOf(ByVal item As RoutingGridPoint) As Integer Implements System.Collections.Generic.IList(Of RoutingGridPoint).IndexOf
        Throw New NotSupportedException("indexof")

    End Function

    Public Sub Insert(ByVal index As Integer, ByVal item As RoutingGridPoint) Implements System.Collections.Generic.IList(Of RoutingGridPoint).Insert
        Throw New NotSupportedException("insert")

    End Sub

    Default Public Property Item(ByVal index As Integer) As RoutingGridPoint Implements System.Collections.Generic.IList(Of RoutingGridPoint).Item
        Get
            Throw New NotSupportedException("item(index)")

        End Get
        Set(ByVal value As RoutingGridPoint)
            Throw New NotSupportedException("item(index)")

        End Set
    End Property

    Public Sub RemoveAt(ByVal index As Integer) Implements System.Collections.Generic.IList(Of RoutingGridPoint).RemoveAt
        Throw New NotSupportedException("removeat")

    End Sub

    Public Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
        Throw New NotSupportedException("removeat")

    End Function
End Class
