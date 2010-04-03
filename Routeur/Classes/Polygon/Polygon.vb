Public Class Polygon

    Implements IList(Of Coords)

    Private Const BASE_ARRAY_SIZE As Integer = 1000
    Private Const BASE_ARRAY_EXTENSION As Integer = 100

    Private Structure CoordsArray
        Public Coords() As Coords
    End Structure

    Private _Data() As CoordsArray
    Private _Count As Integer = 0

    Private Sub CheckSlotIndex(ByVal SlotIndex As Integer)
        Dim Slot As Integer = SlotIndex \ BASE_ARRAY_SIZE

        If _Data Is Nothing OrElse _Data.Length <= Slot Then
            ReDim Preserve _Data(Slot)
            _Data(Slot) = New CoordsArray
        End If

        With _Data(Slot)
            Dim index As Integer = SlotIndex Mod BASE_ARRAY_SIZE
            If .Coords Is Nothing OrElse .Coords.Length <= index Then

                ReDim Preserve .Coords((SlotIndex \ BASE_ARRAY_EXTENSION + 1) * BASE_ARRAY_EXTENSION)

            End If
        End With
    End Sub


    Public Sub Add(ByVal item As Coords) Implements System.Collections.Generic.ICollection(Of Coords).Add

        CheckSlotIndex(_Count)
        _Data(_Count \ BASE_ARRAY_SIZE).Coords(_Count Mod BASE_ARRAY_SIZE) = item

        _Count += 1
        Return



    End Sub

    Public Sub Clear() Implements System.Collections.Generic.ICollection(Of Coords).Clear

        If Not _Data Is Nothing Then
            For Each Slot In _Data
                ReDim Slot.Coords(-1)
            Next
            ReDim _Data(-1)
        End If

        _Count = 0
    End Sub

    Public Function Contains(ByVal item As Coords) As Boolean Implements System.Collections.Generic.ICollection(Of Coords).Contains
        Throw New NotSupportedException("Contains")
    End Function

    Public Shared Sub Copy(ByVal SrcPolygon As Polygon, ByVal DestPolygon As Polygon, ByVal Count As Integer)

        DestPolygon.Clear()
        For i = 0 To Count - 1
            DestPolygon.Add(SrcPolygon(i))
        Next

    End Sub

    Public Sub CopyTo(ByVal array() As Coords, ByVal arrayIndex As Integer) Implements System.Collections.Generic.ICollection(Of Coords).CopyTo
        Throw New NotSupportedException("Contains")


    End Sub

    Public ReadOnly Property Count() As Integer Implements System.Collections.Generic.ICollection(Of Coords).Count
        Get
            Return _Count
        End Get
    End Property

    Public ReadOnly Property IsReadOnly() As Boolean Implements System.Collections.Generic.ICollection(Of Coords).IsReadOnly
        Get
            Return False
        End Get
    End Property

    Public Function Remove(ByVal item As Coords) As Boolean Implements System.Collections.Generic.ICollection(Of Coords).Remove
        Throw New NotSupportedException("Contains")

    End Function

    Public Function IndexOf(ByVal item As Coords) As Integer Implements System.Collections.Generic.IList(Of Coords).IndexOf
        Throw New NotSupportedException("Contains")

    End Function

    Public Sub Insert(ByVal index As Integer, ByVal item As Coords) Implements System.Collections.Generic.IList(Of Coords).Insert
        Throw New NotSupportedException("Contains")

    End Sub

    Default Public Property Item(ByVal index As Integer) As Coords Implements System.Collections.Generic.IList(Of Coords).Item
        Get
            If index > Count OrElse index < 0 Then
                Throw New InvalidOperationException("Index out of bounds")
            End If

            If _Data Is Nothing Then
                Return Nothing
            End If
            Return _Data(index \ BASE_ARRAY_SIZE).Coords(index Mod BASE_ARRAY_SIZE)
        End Get
        Set(ByVal value As Coords)
            CheckSlotIndex(index)
            _Data(index \ BASE_ARRAY_SIZE).Coords(index Mod BASE_ARRAY_SIZE) = value

            If index > _Count Then
                _Count = index
            End If

        End Set
    End Property

    Public Sub RemoveAt(ByVal index As Integer) Implements System.Collections.Generic.IList(Of Coords).RemoveAt
        Throw New NotSupportedException("Contains")

    End Sub

    Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of Coords) Implements System.Collections.Generic.IEnumerable(Of Coords).GetEnumerator
        Return New PolygonEnumerator(Me)
    End Function

    Public Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
        Throw New NotSupportedException("Contains")

    End Function

    Public ReadOnly Property Length() As Integer
        Get
            Return Count
        End Get
    End Property
End Class
