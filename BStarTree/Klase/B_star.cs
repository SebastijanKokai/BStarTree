using System;
using System.Collections.Generic;

public class B_star
{

    private Node root;

    private int maxDegree;

    private class Node
    {
        int maxDegree;

        public Node[] Children { get; set; }

        public Node Parent { get; set; }

        public int IndexChildOfParent { get; set; } = -1; //which index is he for his parent

        public int[] Keys { get; set; }

        public bool IsLeaf { get; set; }

        public bool IsFull { get; set; }

        public int Size { get; set; }

        public Node(int degree)
        {
            maxDegree = degree;
            Keys = new int[degree - 1];
            Children = new Node[degree];
        }
    }

    public B_star(int maxDegree)
    {
        //B* tree works best if (degree - 1) is divisible by 3
        //because of TwoToThreeSplit -- but it can work with any degree
        if (maxDegree < 4)
        {
            throw new Exception("B tree max degree must be at least 4");
        }

        this.maxDegree = maxDegree;
        root = new Node(maxDegree);
        root.IsLeaf = true;
    }

    public void Insert(int value)
    {
        InsertNode(root, value);
    }

    private void InsertNode(Node parentNode, int value)
    {
        int index = parentNode.Size - 1;

        if(parentNode.Size > 0)
        while (index >= 0 && value < parentNode.Keys[index])
        {
            index--;
        }

        index++;

        if (root.IsFull) //check if root needs to be split
        {
            SplitRoot(parentNode);
            InsertNode(root, value);
        }
        else if (root.IsLeaf) //if root is leaf, just insert the element
        {
            Insert_Non_Full(parentNode, value);
        }
        else if (parentNode.Children[index] != null && parentNode.Children[index].IsLeaf)//when you find the leaf, insert into it
        {
            InsertIntoLeaf(parentNode, index, value);
        }
        else //searching in which node to insert
        {
            InsertNode(parentNode.Children[index], value);
        }
    }


    private void InsertIntoLeaf(Node parentNode, int index, int value)
    {
        if (parentNode.Children[index].IsFull) //node is full in which i want to insert
        {

            if (parentNode.Children[index + 1] != null) //is there a right sibling?
            {
                if (parentNode.Children[index + 1].IsFull)//full, do TwoToThreeSplit with right sibling
                {
                    TwoToThreeSplit(parentNode, index, value, false);
                }
                else //not full, do rotation right
                {
                    RotateR(parentNode, index, value);
                }
            }
            else if (parentNode.Children[index - 1] != null) //no right sibling, is there a left one?
            {
                if (parentNode.Children[index - 1].IsFull)//full, do TwoToThreeSplit with left sibling
                {
                    TwoToThreeSplit(parentNode, index, value, true);
                }
                else //not full, do rotation left
                {
                    RotateL(parentNode, index, value);
                }
            }
        }
        else //node is not full, just insert the value
        {
            Insert_Non_Full(parentNode.Children[index], value);
        }
        
    }

    private void Insert_Non_Full(Node current, int value)
    {
        int i = current.Size - 1;
        int m = current.Size;

        //when i'm inserting into branch (meaning it has children)
        if (!current.IsLeaf)
        while (i >= 0 && value < current.Keys[i])
        {
            current.Keys[i + 1] = current.Keys[i];
            current.Children[i + 2] = current.Children[i + 1];
            i -= 1;
        }
        //when i'm inserting into leaf
        else
        while (i >= 0 && value < current.Keys[i])
        {
            current.Keys[i + 1] = current.Keys[i];
            i -= 1;
        }
        current.Keys[i + 1] = value;
        current.Size++;

        //move children if not leaf
        if (!current.IsLeaf)
        {
            int x = i + 1;
            int c = i + 2;
            current.Children[c] = current.Children[x];
            current.Children[x] = new Node(maxDegree);

            int j = 0;
            for(; current.Children[c].Keys[j] < value; j++)
            {
                current.Children[x].Keys[j] = current.Children[c].Keys[j];
                if(!current.Children[c].IsLeaf)
                {
                    current.Children[x].Children[j] = current.Children[c].Children[j];
                }

                current.Children[c].Size--;
                current.Children[x].Size++;
            }
            int k;
            for (k = j; k < m; k++)
            {
                current.Children[c].Keys[k - j] = current.Children[c].Keys[k];
                current.Children[c].Keys[k] = 0;
                if (!current.Children[c].IsLeaf)
                { 
                    current.Children[c].Children[k - j] = current.Children[c].Children[k];
                    current.Children[c].Children[k] = null;
                } 
            }

            if (!current.Children[c].IsLeaf)
            {
                current.Children[c].Children[current.Children[c].Size] = current.Children[c].Children[k - j];
            }
        }

        //if he is full, set his property IsFull to true
        if (current.Size == maxDegree - 1)
            current.IsFull = true;
    }

    private void RotateR(Node parentNode, int indexChild, int value)
    {
        int indexParentKey;
        int nodeSize = parentNode.Children[indexChild].Size;

        if (indexChild == 0)
            indexParentKey = 0;
        else if (indexChild == nodeSize)
            indexParentKey = nodeSize - 1;
        else
            indexParentKey = indexChild;

        int nodeSiblingSize = parentNode.Children[indexChild + 1].Size;

        //Move elements of right sibling to the right, so i make space for 0 index
        for (int j = nodeSiblingSize - 1; j >= 0; j--) 
        {
            parentNode.Children[indexChild + 1].Keys[j + 1] = parentNode.Children[indexChild + 1].Keys[j];
        }

        //If he is not a leaf, i have to move children also
        if (!parentNode.Children[indexChild].IsLeaf)
        {
            for (int j = nodeSiblingSize; j >= 0; j--)
                parentNode.Children[indexChild + 1].Children[j + 1] = parentNode.Children[indexChild + 1].Children[j];

            //Last child of full node going to the first child of right node
            parentNode.Children[indexChild + 1].Children[0] = parentNode.Children[indexChild].Children[nodeSize];
            parentNode.Children[indexChild].Children[nodeSize] = null;
        }

        //From parent node to right child
        parentNode.Children[indexChild + 1].Keys[0] = parentNode.Keys[indexParentKey];

        //change size
        parentNode.Children[indexChild + 1].Size++;

        //Biggest from left child to parent node, insert into right place
        if (parentNode.Children[indexChild].IsLeaf &&
            value < parentNode.Keys[indexParentKey] && value > parentNode.Children[indexChild].Keys[nodeSize - 1])
        {
            parentNode.Keys[indexParentKey] = value;
        }
        else
        {
            parentNode.Keys[indexParentKey] = parentNode.Children[indexChild].Keys[nodeSize - 1];

            //Deleting the biggest from left child
            parentNode.Children[indexChild].Keys[nodeSize - 1] = 0;
            parentNode.Children[indexChild].Size--;
            Insert_Non_Full(parentNode.Children[indexChild], value);
        }

        //If sibling doesn't have space, set his property accordingly
        if (parentNode.Children[indexChild + 1].Size == (nodeSize))
            parentNode.Children[indexChild + 1].IsFull = true;
    }

    private void RotateL(Node parentNode, int indexChild, int value)
    {
        int indexParentKey;

        if (indexChild == 0)
            indexParentKey = 0;
        else if (indexChild == maxDegree)
            indexParentKey = maxDegree - 1;
        else
            indexParentKey = indexChild - 1;

        //Element from parent to left child
        Insert_Non_Full(parentNode.Children[indexChild - 1], parentNode.Keys[indexParentKey]);

        if (parentNode.Children[indexChild].IsLeaf &&
            value > parentNode.Keys[indexParentKey] && value < parentNode.Children[indexChild].Keys[0])
        {
            //put value if parent < value < biggest from right sibling
            parentNode.Keys[indexParentKey] = value;
        }
        else
        {
            //From full node, put smallest into parent
            parentNode.Keys[indexParentKey] = parentNode.Children[indexChild].Keys[0];

            //Pomeri za jedno polje ulevo pun cvor
            //Move to the left elements of full node
            int nodeSize = parentNode.Children[indexChild].Size;
            for (int i = 0; i < nodeSize - 1; i++)
                parentNode.Children[indexChild].Keys[i] = parentNode.Children[indexChild].Keys[i + 1];

            //If not leaf, i have to move children also
            //move children to the left by 1 field
            if (!parentNode.Children[indexChild].IsLeaf)
            {
                //Move first child from full node, to the last child of left sibling
                parentNode.Children[indexChild].Children[nodeSize] = parentNode.Children[indexChild].Children[0];
                for (int i = 0; i < nodeSize - 1; i++)
                    parentNode.Children[indexChild].Children[i] = parentNode.Children[indexChild].Children[i + 1];
            }

            //Delete last element from full node
            parentNode.Children[indexChild].Keys[nodeSize - 1] = 0;

            //change size
            parentNode.Children[indexChild].Size--;
            parentNode.Children[indexChild].IsFull = false;

            Insert_Non_Full(parentNode.Children[indexChild], value);

        }
    }
    private void TwoToThreeSplit(Node parentNode, int indexFullChild, int value, bool isLeftSibling)
    {
        //if he has a parent, he isn't root
        int leftSideIndex = isLeftSibling ? (indexFullChild - 1) : indexFullChild;
        int rightSideIndex = isLeftSibling ? (indexFullChild) : indexFullChild + 1;
        int twoThirds = (maxDegree * 2) / 3;

        int parentKeyIndex = isLeftSibling ? (indexFullChild - 1) : (indexFullChild);
        int nodeMaxSize = maxDegree - 1;
        bool isLeaf = parentNode.Children[indexFullChild].IsLeaf;

        //elements of two nodes + parent item + value
        int j = 0;
        int[] arr = new int[maxDegree * 2];
        Node[] children = new Node[maxDegree * 2];                

        //the order doesn't matter, they are gonna be sorted anyways
        for (int i = 0; i < nodeMaxSize; i++)
        {
            arr[j++] = parentNode.Children[leftSideIndex].Keys[i];
            arr[j++] = parentNode.Children[rightSideIndex].Keys[i];
        }
        arr[j++] = value;
        arr[j++] = parentNode.Keys[parentKeyIndex];

        //if it isn't a leaf, i have to take the children also
        if(!isLeaf)
        {
            int x = 0;
            for(int i = 0; i < maxDegree; i++)
            {
                children[x++] = parentNode.Children[leftSideIndex].Children[i];
            }
            for (int i = 0; i < maxDegree; i++)
            {
                children[x++] = parentNode.Children[rightSideIndex].Children[i];
            }
        }

        BubbleSort(arr);
       // int indexOfElement = BinarySearch(arr, value);
        j = 0;
        int k;

        //1st two thirds
        for(k = 0; k < twoThirds; k++)
        {
            parentNode.Children[leftSideIndex].Keys[k] = arr[j++];
        }
        //changing the size
        while (k < nodeMaxSize)
        {
            parentNode.Children[leftSideIndex].Keys[k++] = 0;
            parentNode.Children[leftSideIndex].Size--;
        }
        parentNode.Children[leftSideIndex].IsFull = false;

        //first parent element
        int parent1 = arr[j++];

        parentNode.Keys[parentKeyIndex] = parent1;

        //2nd two thirds
        for (k = 0; k < twoThirds; k++)
        {
            parentNode.Children[rightSideIndex].Keys[k] = arr[j++];
        }
        //changing the size
        while (k < nodeMaxSize)
        {
            parentNode.Children[rightSideIndex].Keys[k++] = 0;
            parentNode.Children[rightSideIndex].Size--;
        }
        parentNode.Children[rightSideIndex].IsFull = false;

        //second parent element
        int parent2 = arr[j++];

        //new node
        Node newNode = new Node(maxDegree);

        //3rd two thirds
        for (k = 0; j < maxDegree * 2; k++)
        {
            newNode.Keys[k] = arr[j++];
            newNode.Size++;
        }

        //parent will change depending on the if
        Node rightNode = parentNode.Children[rightSideIndex];

        //inserting element into parent node
        if (parentNode.IsFull) //parent is full
        {
            if (parentNode.Parent == null) //parent is root
            {
                SplitRoot(parentNode);
            }
            else //parent is not root
                InsertIntoLeaf(parentNode.Parent, parentNode.IndexChildOfParent, parent2);
        }
        else //parent is not full, just insert the element
        {
            //move all elements by 1 to the right of parent node -- keys and children
            int m = rightNode.Parent.Size - 1;
            for (; m > parentKeyIndex; m--)
            {
                rightNode.Parent.Keys[m + 1] = rightNode.Parent.Keys[m];
                if (!isLeaf)
                {
                    rightNode.Parent.Children[m + 2] = rightNode.Parent.Children[m + 1];
                }
            }
            rightNode.Parent.Keys[m + 1] = parent2;
            rightNode.Parent.Children[m + 2] = newNode;
            rightNode.Parent.Size++;

            newNode.Parent = rightNode.Parent;
            newNode.IsLeaf = isLeaf;
        }

        

        if (rightNode.Parent.Size == maxDegree - 1)
            rightNode.Parent.IsFull = true;

       // parentNode.Children[rightSideIndex + 1] = newNode;


    }

    private void SplitRoot(Node current)
    {
        int middleIndex = current.Size / 2;

        Node newRoot = new Node(maxDegree);
        Node newNode = new Node(maxDegree);

        //from current node to new node
        int j = 0;
        int i = middleIndex + 1;
        if (!current.IsLeaf)
        {
            
            while (i < maxDegree - 1)
            {
                //moving keys and children
                newNode.Keys[j] = current.Keys[i];
                newNode.Children[j] = current.Children[i];
                newNode.Children[j].Parent = newNode;
                //deleting children and keys after moving them
                current.Keys[i] = 0;
                current.Children[i] = null;
                current.Size--;
                //incrementing size of new node, and decrementing from current
                newNode.Size++;
                j++; i++;
            }
            newNode.Children[j] = current.Children[i];
            current.Children[i] = null;
        }
        else
        {
            while(i < maxDegree - 1)
            {
                newNode.Keys[j] = current.Keys[i];
                current.Keys[i] = 0;
                current.Size--;
                newNode.Size++;
                i++; j++;
            }
            //they are leaves if root was leaf
            newNode.IsLeaf = true;
            current.IsLeaf = true;
        }

        //changing parent and indexchildofparent (to which index does child belong to)
        current.Parent = newRoot;
        current.IndexChildOfParent = 0;

        newNode.Parent = newRoot;
        newNode.IndexChildOfParent = 1;

        newRoot.Parent = null;
        //ubacivanje srednjeg elementa u root, i postavljanje njegove dece
        //inserting middle element into root, and setting his children
        newRoot.Keys[0] = current.Keys[middleIndex];
        newRoot.Children[0] = current;
        newRoot.Children[1] = newNode;
        newRoot.Size++;
        //deletion of middle element from current
        current.Keys[middleIndex] = 0;
        current.Size--;
        current.IsFull = false;

        //newRoot becomes the new root
        this.root = newRoot;
    }

    public bool Find(int value) //if value exists, return true
    {
        Node result = FindNode(root, value);
        if (result != null)
        {
            return true;
        }
        return false;
    }

    private Node FindNode(Node current, int value)
    {
        Node temp = current;

        int i = 0;

        while (i <= current.Size - 1 && value > current.Keys[i])
        {
            i++;
        }

        if (i <= current.Size - 1 && value == current.Keys[i])
        {
            return current;
        }
        else if (current.IsLeaf)
        {
            return null;
        }
        else
        {
            return FindNode(current.Children[i], value);
        }
    }

    public void Print()
    {
        InOrderTraversal(root);
    }

    private void InOrderTraversal(Node current) //inserting all the nodes
    {
        if (!current.IsLeaf)
        {
            for (int i = 0; current.Children[i] != null; i++)
            {
                InOrderTraversal(current.Children[i]);
            }
        }
        for(int j = 0; j < current.Keys.Length; j++)
        Console.Write(current.Keys[j] + " ");
        Console.WriteLine("\n--");
    }

    private void InsertRecursion(Node current, int value, int c, int x) //testing something
    {
        current.Children[c] = current.Children[x];
        current.Children[x] = new Node(maxDegree);

        int j = 0;
        //move all values < value from right node to left node
        for (; current.Children[c].Keys[j] < value; j++)
        {
            current.Children[x].Keys[j] = current.Children[c].Keys[j];
            if (!current.Children[c].IsLeaf)
            {
                current.Children[x].Children[j] = current.Children[c].Children[j];
            }

            current.Children[c].Size--;
            current.Children[x].Size++;
        }
        int k = 0;
        //pomeraj ulevo kljuceva i dece desnog cvora
        //move to the left keys and children of right node
        //for (k = j; k < m; k++)
        //{
        //    current.Children[c].Keys[k - j] = current.Children[c].Keys[k];
        //    current.Children[c].Keys[k] = 0;
        //    if (!current.Children[c].IsLeaf)
        //    {
        //        current.Children[c].Children[k - j] = current.Children[c].Children[k];
        //        current.Children[c].Children[k] = null;
        //    }
        //}

        if (!current.Children[c].IsLeaf)
        {
            current.Children[c].Children[current.Children[c].Size] = current.Children[c].Children[k - j];
           // InsertRecursion()
        }
    }

    //sort needed for TwoToThreeSplit
    private void BubbleSort(int[] arr)
    {
        int temp = 0;

        for (int i = 0; i < arr.Length; i++)
        {
            for (int j = 0; j < arr.Length - 1; j++)
            {
                if (arr[j] > arr[j + 1])
                {
                    temp = arr[j + 1];
                    arr[j + 1] = arr[j];
                    arr[j] = temp;
                }
            }
        }
    }

    public int BinarySearch(int[] arr, int key) //haven't used it yet
    {
        int min = 0;
        int max = arr.Length - 1;
        while (min <= max)
        {
            int mid = (min + max) / 2;
            if (key == arr[mid])
            {
                return ++mid;
            }
            else if (key < arr[mid])
            {
                max = mid - 1;
            }
            else
            {
                min = mid + 1;
            }
        }
        return -1;
    }
}