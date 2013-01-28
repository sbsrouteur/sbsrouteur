Public Class BSPList

    Implements IList(Of ICoords)
    Implements IEnumerator

    Private _Node As ICoords
    Private _SubNodes(3) As BSPList
    Private _EnumIndex As Integer
    Private _SubEnum As IEnumerator

    Public Sub Add(ByVal item As ICoords) Implements System.Collections.Generic.ICollection(Of ICoords).Add

        If _Node Is Nothing Then
            _Node = item
        ElseIf _Node.Equals(item) Then
            Throw New NotSupportedException("Duplicate item in tree")
        Else
            Dim I As Integer = SubNodeIndex(item)

            If _SubNodes(I) Is Nothing Then
                _SubNodes(I) = New BSPList
            End If
            _SubNodes(I).Add(item)
        End If

    End Sub

    Public Sub Clear() Implements System.Collections.Generic.ICollection(Of ICoords).Clear

        _Node = Nothing

        Dim i As Integer
        For i = 0 To 3
            If _SubNodes(i) IsNot Nothing Then
                _SubNodes(i).Clear()
                _SubNodes(i) = Nothing
            End If
        Next

    End Sub

    Public Function Contains(ByVal item As ICoords) As Boolean Implements System.Collections.Generic.ICollection(Of ICoords).Contains

        If _Node Is Nothing Then
            Return False
        ElseIf CoordsComparer.Equals1(_Node, item) Then
            Return True
        Else
            Dim i As Integer = SubNodeIndex(item)

            If _SubNodes(i) Is Nothing Then
                Return False
            Else
                Return _SubNodes(i).Contains(item)
            End If
        End If

    End Function

    Public Sub CopyTo(ByVal array() As ICoords, ByVal arrayIndex As Integer) Implements System.Collections.Generic.ICollection(Of ICoords).CopyTo

    End Sub

    Public ReadOnly Property Count() As Integer Implements System.Collections.Generic.ICollection(Of ICoords).Count
        Get
            Dim CurCount As Integer = 0

            If _Node IsNot Nothing Then
                CurCount = 1
                For Each node In _SubNodes
                    If node IsNot Nothing Then
                        CurCount += node.Count
                    End If
                Next

            End If

            Return CurCount
        End Get
    End Property

    Public ReadOnly Property IsReadOnly() As Boolean Implements System.Collections.Generic.ICollection(Of ICoords).IsReadOnly
        Get
            Return False
        End Get
    End Property

    Public Function Remove(ByVal item As ICoords) As Boolean Implements System.Collections.Generic.ICollection(Of ICoords).Remove
        Throw New NotSupportedException("Remove")
    End Function

    Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of ICoords) Implements System.Collections.Generic.IEnumerable(Of ICoords).GetEnumerator

        Throw New NotSupportedException("Get enumerator")
        'Return Me

    End Function

    Public Function IndexOf(ByVal item As ICoords) As Integer Implements System.Collections.Generic.IList(Of ICoords).IndexOf

        If _Node Is Nothing Then
            Throw New NotSupportedException("Item not in bsp")
        ElseIf _Node.Equals(item) Then
        End If

    End Function

    Public Sub Insert(ByVal index As Integer, ByVal item As ICoords) Implements System.Collections.Generic.IList(Of ICoords).Insert
        Throw New NotSupportedException("Insert not supported")
    End Sub

    Default Public Property Item(ByVal index As Integer) As ICoords Implements System.Collections.Generic.IList(Of ICoords).Item
        Get

            Throw New NotSupportedException("Set item value")
            

        End Get
        Set(ByVal value As ICoords)
            Throw New NotSupportedException("Set item value")
        End Set
    End Property

    Default Public Property Item(ByVal itemtofind As ICoords) As ICoords
        Get
            If _Node Is Nothing Then
                Throw New NotSupportedException("Item not found")
            ElseIf CoordsComparer.Equals1(_Node, itemtofind) Then
                Return _Node
            Else
                Dim I As Integer = SubNodeIndex(itemtofind)

                If _SubNodes(I) Is Nothing Then
                    Throw New NotSupportedException("Item not found")
                Else
                    Return _SubNodes(I)(itemtofind)
                End If
            End If


        End Get
        Set(ByVal value As ICoords)
            Throw New NotSupportedException("Set item value")
        End Set
    End Property

    Public Sub RemoveAt(ByVal index As Integer) Implements System.Collections.Generic.IList(Of ICoords).RemoveAt

    End Sub

    Private Function SubNodeIndex(ByVal Item As ICoords) As Integer

        If _Node Is Nothing Then
            Throw New NotSupportedException("Should not GetSubnode when no value is defined!!!")
        End If
        Dim x As Integer

        If Item.Lon > _Node.Lon Then
            x = 1
        Else
            x = 0
        End If

        If Item.Lat > _Node.Lat Then
            Return 2 + x
        Else
            Return x
        End If
    End Function

    Public Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
        Throw New NotSupportedException("No get enumerator")
        Return Me
    End Function

    Public ReadOnly Property Current() As Object Implements System.Collections.IEnumerator.Current
        Get

            If _EnumIndex = 0 Then
                Return _Node
            Else
                Return _SubNodes(_EnumIndex)
            End If


        End Get
    End Property

    Public Function MoveNext() As Boolean Implements System.Collections.IEnumerator.MoveNext

        If _EnumIndex < 4 Then
            _EnumIndex += 1
            Return True
        Else
            Return False
        End If

    End Function

    Public Sub Reset() Implements System.Collections.IEnumerator.Reset

        _EnumIndex = -1

    End Sub
End Class
