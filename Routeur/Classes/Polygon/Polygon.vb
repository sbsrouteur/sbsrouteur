'This file is part of Routeur.
'Copyright (C) 2011  sbsRouteur(at)free.fr

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

Public Class Polygon

    Implements IList(Of Coords)

    Private Const BASE_ARRAY_SIZE As Integer = 1750

    Private Class CoordsList
        Private _Coords(BASE_ARRAY_SIZE - 1) As Coords
        Private _NextList As CoordsList = Nothing


        Public Function Length() As Long
            If _NextList Is Nothing Then
                Return BASE_ARRAY_SIZE
            Else
                Return BASE_ARRAY_SIZE + _NextList.Length
            End If
        End Function

        Public Sub Extend()
            If _NextList Is Nothing Then
                _NextList = New CoordsList
            Else
                _NextList.Extend()
            End If
        End Sub

        Public Sub clear()

            ReDim _Coords(-1)
            If Not _NextList Is Nothing Then
                _NextList.clear()
                _NextList = Nothing
            End If

        End Sub

        Public Property Coords(ByVal index As Integer) As Coords
            Get
                If index >= BASE_ARRAY_SIZE AndAlso Not _NextList Is Nothing Then
                    Return _NextList.Coords(index - BASE_ARRAY_SIZE)
                ElseIf index < BASE_ARRAY_SIZE Then
                    Return _Coords(index)
                Else
                    Throw New InvalidOperationException("index over array size!")
                End If

            End Get
            Set(ByVal value As Coords)
                If index >= BASE_ARRAY_SIZE AndAlso Not _NextList Is Nothing Then
                    _NextList.Coords(index - BASE_ARRAY_SIZE) = value
                ElseIf index < BASE_ARRAY_SIZE Then
                    _Coords(index) = value
                Else
                    Extend()
                    Coords(index) = value
                End If
            End Set
        End Property

    End Class

    Private _Data As CoordsList
    Private _Count As Integer = 0

    Private Sub CheckSlotIndex(ByVal SlotIndex As Integer)

        If _Data Is Nothing Then
            _Data = New CoordsList
        End If
        If _Data.Length <= SlotIndex Then
            _Data.Extend()
        End If

    End Sub


    Public Sub Add(ByVal item As Coords) Implements System.Collections.Generic.ICollection(Of Coords).Add

        CheckSlotIndex(_Count)
        _Data.Coords(_Count) = item
        _Count += 1
        Return



    End Sub

    Public Sub Clear() Implements System.Collections.Generic.ICollection(Of Coords).Clear

        If Not _Data Is Nothing Then
            _Data.clear()
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
            Return _Data.Coords(index)
        End Get
        Set(ByVal value As Coords)
            CheckSlotIndex(index)
            _Data.Coords(index) = value

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
