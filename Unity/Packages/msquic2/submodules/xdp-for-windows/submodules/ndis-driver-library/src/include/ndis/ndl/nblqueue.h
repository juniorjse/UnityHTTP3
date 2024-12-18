/*++

    Copyright (c) Microsoft. All rights reserved.

    This code is licensed under the MIT License.
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF
    ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
    TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
    PARTICULAR PURPOSE AND NONINFRINGEMENT.

Module Name:

    nblqueue.h

Provenance:

    Version 1.1.0 from https://github.com/microsoft/ndis-driver-library

Abstract:

    Defines the NBL_QUEUE and utility functions to operate on it

    NBLs are typically linked into a singly-linked list.  Because it's a
    singly-linked list, appending NBLs is a slow O(n) operation.  This module
    defines the NBL_QUEUE datastructure, which is designed to allow efficient
    O(1) appends.

    The NBL_COUNTED_QUEUE is an NBL_QUEUE, but also adds the additional feature
    of tracking how many NBLs are in the queue.

Example usage:

    NET_BUFFER_LIST *NblChain1 = //  A=>B=>C=>NULL
    NET_BUFFER_LIST *NblChain2 = //  D=>NULL
    NBL_QUEUE NblQueue;

    NdisInitializeNblQueue(&NblQueue);
    // NblQueue is now empty

    NdisAppendNblChainToNblQueue(&NblQueue, NblChain1);
    // NblQueue now has three NBLs in it: A, B, and C

    NdisAppendNblChainToNblQueue(&NblQueue, NblChain2);
    // NblQueue now has four NBLs in it: A, B, C, and D

    NET_BUFFER_LIST *NblChain3 = NdisPopAllFromNblQueue(&NblQueue);
    // NblQueue is now empty
    // NblChain3 is A=>B=>C=>D=>NULL

Programming languages:

    All the features of this header are available to C language code.  However,
    users of C++ may wish to consume the features via a C++ wrapper class.  The
    wrapper class provides automatic initialization of structures and a few
    convenient member functions.

    The example usage presented above could also be written in C++ as follows:

    NET_BUFFER_LIST *NblChain1 = //  A=>B=>C=>NULL
    NET_BUFFER_LIST *NblChain2 = //  D=>NULL
    ndis::nbl_queue NblQueue;

    NblQueue.append_slow(NblChain1);
    // NblQueue now has three NBLs in it: A, B, and C

    NblQueue.append_slow(NblChain2);
    // NblQueue now has four NBLs in it: A, B, C, and D

    NET_BUFFER_LIST *NblChain3 = NblQueue.clear();
    // NblQueue is now empty
    // NblChain3 is A=>B=>C=>D=>NULL

Table of Contents:

    Routines for NBL_QUEUEs:
        NdisInitializeNblQueue
        NdisIsNblQueueEmpty
        NdisGetNblChainFromNblQueue
        NdisAppendNblChainToNblQueueFast
        NdisAppendNblQueueToNblQueueFast
        NdisAppendNblChainToNblQueue
        NdisAppendSingleNblToNblQueue
        NdisPopAllFromNblQueue
        NdisPopFirstNblFromNblQueue

    Routines for NBL_COUNTED_QUEUEs:
        NdisInitializeNblCountedQueue
        NdisIsNblCountedQueueEmpty
        NdisGetNblChainFromNblCountedQueue
        NdisAppendNblChainToNblCountedQueueFast
        NdisAppendNblCountedQueueToNblCountedQueueFast
        NdisAppendNblChainToNblCountedQueue
        NdisAppendSingleNblToNblCountedQueue
        NdisPopAllFromNblCountedQueue
        NdisPopFirstNblFromNblCountedQueue

Environment:

    Kernel mode

--*/

#pragma once

#pragma region System Family (kernel drivers) with Desktop Family for compat
#include <winapifamily.h>
#if WINAPI_FAMILY_PARTITION(WINAPI_PARTITION_SYSTEM | WINAPI_PARTITION_DESKTOP)

#include <ndis/ndl/nblchain.h>

typedef struct NBL_QUEUE_t
{
    // Pointer to first NBL in chain, or NULL if the queue is empty
    NET_BUFFER_LIST *First;

    // Pointer to last NBL in chain, or to this->First if queue is empty
    NET_BUFFER_LIST **Last;

#ifdef __cplusplus

    NBL_QUEUE_t() = default;
    ~NBL_QUEUE_t() = default;

    // Do not copy this data structure; it takes internal pointers to itself,
    // so cannot be trivially copied.
    NBL_QUEUE_t(NBL_QUEUE_t &) = delete;
    NBL_QUEUE_t &operator=(NBL_QUEUE_t &) = delete;

    // Move is okay; use with std::move.
    inline NBL_QUEUE_t(NBL_QUEUE_t &&rhs);
    inline NBL_QUEUE_t &operator=(NBL_QUEUE_t &&rhs);

#endif // __cplusplus
} NBL_QUEUE;

typedef struct NBL_COUNTED_QUEUE_t
{
    // Contains 0 or more NBLs
    NBL_QUEUE Queue;

    // The number of NBLs in the Queue
    SIZE_T NblCount;

#ifdef __cplusplus

    NBL_COUNTED_QUEUE_t() = default;
    ~NBL_COUNTED_QUEUE_t() = default;

    // Do not copy this data structure; it takes internal pointers to itself,
    // so cannot be trivially copied.
    NBL_COUNTED_QUEUE_t(NBL_COUNTED_QUEUE_t &) = delete;
    NBL_COUNTED_QUEUE_t &operator=(NBL_COUNTED_QUEUE_t &) = delete;

    // Move is okay; use with std::move.
    inline NBL_COUNTED_QUEUE_t(NBL_COUNTED_QUEUE_t &&rhs);
    inline NBL_COUNTED_QUEUE_t &operator=(NBL_COUNTED_QUEUE_t &&rhs);

#endif // __cplusplus
} NBL_COUNTED_QUEUE;

_IRQL_requires_max_(DISPATCH_LEVEL)
inline
void
NDIS_ASSERT_VALID_NBL_QUEUE(
    _In_ NBL_QUEUE const *NblQueue)
/*++

Routine Description:

    Verifies the NBL_QUEUE appears to be correct

Arguments:

    NblQueue

--*/
{
#if DBG

    NDIS_ASSERT(NblQueue->Last != NULL);

    if (NblQueue->First == NULL)
    {
        NDIS_ASSERT(NblQueue->Last == &NblQueue->First);
    }
    else
    {
        NET_BUFFER_LIST *Last = NdisLastNblInNblChain(NblQueue->First);
        NDIS_ASSERT(NblQueue->Last == &Last->Next);
    }

#else
    UNREFERENCED_PARAMETER(NblQueue);
#endif
}

_IRQL_requires_max_(DISPATCH_LEVEL)
inline
void
NDIS_ASSERT_VALID_NBL_COUNTED_QUEUE(
    _In_ NBL_COUNTED_QUEUE const *CountedQueue)
/*++

Routine Description:

    Verifies the NBL_COUNTED_QUEUE appears to be correct

Arguments:

    CountedQueue

--*/
{
#if DBG

    const SIZE_T Count = NdisNumNblsInNblChain(CountedQueue->Queue.First);

    NDIS_ASSERT_VALID_NBL_QUEUE(&CountedQueue->Queue);
    NDIS_ASSERT(Count == CountedQueue->NblCount);

#else
    UNREFERENCED_PARAMETER(CountedQueue);
#endif
}

_IRQL_requires_max_(DISPATCH_LEVEL)
inline
void
NdisInitializeNblQueue(
    _Out_ NBL_QUEUE *NblQueue)
/*++

Routine Description:

    Initializes an NBL_QUEUE datastructure

Arguments:

    NblQueue

--*/
{
    NblQueue->First = NULL;

    { C_ASSERT(FIELD_OFFSET(NET_BUFFER_LIST, Next) == 0); }

    NblQueue->Last = &NblQueue->First;

    NDIS_ASSERT_VALID_NBL_QUEUE(NblQueue);
}

_IRQL_requires_max_(DISPATCH_LEVEL)
inline
void
NdisInitializeNblCountedQueue(
    _Out_ NBL_COUNTED_QUEUE *CountedQueue)
/*++

Routine Description:

    Initializes an NBL_COUNTED_QUEUE datastructure

Arguments:

    CountedQueue

--*/
{
    NdisInitializeNblQueue(&CountedQueue->Queue);
    CountedQueue->NblCount = 0;

    NDIS_ASSERT_VALID_NBL_COUNTED_QUEUE(CountedQueue);
}

_IRQL_requires_max_(DISPATCH_LEVEL)
inline
BOOLEAN
NdisIsNblQueueEmpty(
    _In_ NBL_QUEUE const *NblQueue)
/*++

Routine Description:

    Determines whether any NBLs are in the queue

Arguments:

    NblQueue

Return Value:

    FALSE if there is at least one NBL in the queue, else
    TRUE if there are no NBLs in the queue

--*/
{
    NDIS_ASSERT_VALID_NBL_QUEUE(NblQueue);

    return NblQueue->First == NULL;
}

_IRQL_requires_max_(DISPATCH_LEVEL)
inline
BOOLEAN
NdisIsNblCountedQueueEmpty(
    _In_ NBL_COUNTED_QUEUE const *CountedQueue)
/*++

Routine Description:

    Determines whether any NBLs are in the queue

Arguments:

    CountedQueue

Return Value:

    FALSE if there is at least one NBL in the queue, else
    TRUE if there are no NBLs in the queue

--*/
{
    NDIS_ASSERT_VALID_NBL_COUNTED_QUEUE(CountedQueue);

    return NdisIsNblQueueEmpty(&CountedQueue->Queue);
}

_IRQL_requires_max_(DISPATCH_LEVEL)
inline
NET_BUFFER_LIST *
NdisGetNblChainFromNblQueue(
    _In_ NBL_QUEUE const *Queue)
/*++

Routine Description:

    Retreives the NBL chain from the queue

Arguments:

    Queue

Return Value:

    The NBL chain stored in the queue, or NULL if the queue is empty

--*/
{
    NDIS_ASSERT_VALID_NBL_QUEUE(Queue);

    return Queue->First;
}

_IRQL_requires_max_(DISPATCH_LEVEL)
inline
NET_BUFFER_LIST *
NdisGetNblChainFromNblCountedQueue(
    _In_ NBL_COUNTED_QUEUE const *CountedQueue)
/*++

Routine Description:

    Retreives the NBL chain from the queue

Arguments:

    CountedQueue

Return Value:

    The NBL chain stored in the queue, or NULL if the queue is empty

--*/
{
    NDIS_ASSERT_VALID_NBL_COUNTED_QUEUE(CountedQueue);

    return NdisGetNblChainFromNblQueue(&CountedQueue->Queue);
}

_IRQL_requires_max_(DISPATCH_LEVEL)
inline
void
NdisAppendNblChainToNblQueueFast(
    _Inout_ NBL_QUEUE *NblQueue,
    _In_ NET_BUFFER_LIST *NblChainFirst,
    _In_ NET_BUFFER_LIST *NblChainLast)
/*++

Routine Description:

    Appends an NBL chain to an NBL_QUEUE

    Executes in O(1) time, so it's "Fast".  But you must know the last
    NBL in the chain.  If you don't know the last NBL in the chain,
    use NdisAppendNblChainToNblQueue instead, which is O(n).

Arguments:

    NblQueue

    NblChainFirst - First NBL in the chain

    NblChainLast - Last NBL in the chain (NblChainLast->Next must be NULL)

--*/
{
    NDIS_ASSERT_VALID_NBL_QUEUE(NblQueue);
#if DBG
    {
        const NET_BUFFER_LIST *Last = NdisLastNblInNblChain(NblChainFirst);
        NDIS_ASSERT(Last == NblChainLast);
        NDIS_ASSERT(Last != *NblQueue->Last);
    }
#endif

    *NblQueue->Last = NblChainFirst;
    NblQueue->Last = &NblChainLast->Next;

    NDIS_ASSERT_VALID_NBL_QUEUE(NblQueue);
}

_IRQL_requires_max_(DISPATCH_LEVEL)
inline
void
NdisAppendNblChainToNblCountedQueueFast(
    _Inout_ NBL_COUNTED_QUEUE *CountedQueue,
    _In_ NET_BUFFER_LIST *NblChainFirst,
    _In_ NET_BUFFER_LIST *NblChainLast,
    _In_ SIZE_T NumNblsToAppend)
/*++

Routine Description:

    Appends an NBL chain to an NBL_COUNTED_QUEUE

    Executes in O(1) time, so it's "Fast".  But you must know the last
    NBL in the chain.  If you don't know the last NBL in the chain,
    use NdisAppendNblChainToNblCountedQueue instead, which is O(n).

Arguments:

    CountedQueue

    NblChainFirst - First NBL in the chain

    NblChainLast - Last NBL in the chain (NblChainLast->Next must be NULL)

    NumNblsToAppend - The number of NBLs in [NblChainFirst, NblChainLast]

--*/
{
    NDIS_ASSERT_VALID_NBL_COUNTED_QUEUE(CountedQueue);

#if DBG
    {
        const SIZE_T VerifiedCount = NdisNumNblsInNblChain(NblChainFirst);

        NDIS_ASSERT(VerifiedCount == NumNblsToAppend);

        // Overflow check
        NDIS_ASSERT(CountedQueue->NblCount + NumNblsToAppend >= NumNblsToAppend);
    }
#endif

    NdisAppendNblChainToNblQueueFast(&CountedQueue->Queue, NblChainFirst, NblChainLast);

    CountedQueue->NblCount += NumNblsToAppend;

    NDIS_ASSERT_VALID_NBL_COUNTED_QUEUE(CountedQueue);
}

_IRQL_requires_max_(DISPATCH_LEVEL)
inline
void
NdisAppendNblQueueToNblQueueFast(
    _Inout_ NBL_QUEUE *Destination,
    _Inout_ NBL_QUEUE *Source)
/*++

Routine Description:

    Removes all the NBLs from Source and appends them to Destination

    N.B. There is no "Slow" version of this routine, but it's named
         "Fast" so that it's clear that it's an O(1) operation, unlike
         NdisAppendNblChainToNblQueue.

Arguments:

    Destination - Receives all the NBLs from Source
    Source - Donates NBLs to Destination; is empty after call returns

--*/
{
    NDIS_ASSERT_VALID_NBL_QUEUE(Destination);
    NDIS_ASSERT_VALID_NBL_QUEUE(Source);

    // No cycles allowed
    NDIS_ASSERT(Destination->Last != Source->Last);

    if (NdisIsNblQueueEmpty(Source))
    {
        return;
    }

    *Destination->Last = Source->First;
    Destination->Last = Source->Last;

    NdisInitializeNblQueue(Source);

    NDIS_ASSERT_VALID_NBL_QUEUE(Destination);
    NDIS_ASSERT_VALID_NBL_QUEUE(Source);
}

_IRQL_requires_max_(DISPATCH_LEVEL)
inline
void
NdisAppendNblCountedQueueToNblCountedQueueFast(
    _Inout_ NBL_COUNTED_QUEUE *Destination,
    _Inout_ NBL_COUNTED_QUEUE *Source)
/*++

Routine Description:

    Removes all the NBLs from Source and appends them to Destination

    N.B. There is no "Slow" version of this routine, but it's named
         "Fast" so that it's clear that it's an O(1) operation, unlike
         NdisAppendNblChainToNblCountedQueue.

Arguments:

    Destination - Receives all the NBLs from Source
    Source - Donates NBLs to Destination; is empty after call returns

--*/
{
    NDIS_ASSERT_VALID_NBL_COUNTED_QUEUE(Destination);
    NDIS_ASSERT_VALID_NBL_COUNTED_QUEUE(Source);

    // Overflow check
    NDIS_ASSERT(Destination->NblCount + Source->NblCount >= Source->NblCount);

    NdisAppendNblQueueToNblQueueFast(&Destination->Queue, &Source->Queue);

    Destination->NblCount += Source->NblCount;
    Source->NblCount = 0;

    NDIS_ASSERT_VALID_NBL_COUNTED_QUEUE(Destination);
    NDIS_ASSERT_VALID_NBL_COUNTED_QUEUE(Source);
}

_IRQL_requires_max_(DISPATCH_LEVEL)
inline
void
NdisAppendNblChainToNblQueue(
    _In_ NBL_QUEUE *NblQueue,
    _In_ NET_BUFFER_LIST *NblChain)
/*++

Routine Description:

    Appends an NBL chain to an NBL_QUEUE

    This routine has the same effect as NdisAppendNblChainToNblQueueFast,
    however it is slower.  Use this routine if you don't have handy a pointer
    to the last NBL in the chain.

Arguments:

    NblQueue
    NblChain

--*/
{
    NdisAppendNblChainToNblQueueFast(NblQueue, NblChain, NdisLastNblInNblChain(NblChain));
}

_IRQL_requires_max_(DISPATCH_LEVEL)
inline
void
NdisAppendNblChainToNblCountedQueue(
    _In_ NBL_COUNTED_QUEUE *CountedQueue,
    _In_ NET_BUFFER_LIST *NblChain)
/*++

Routine Description:

    Appends an NBL chain to an NBL_COUNTED_QUEUE

    This routine has the same effect as NdisAppendNblChainToNblCountedQueueFast,
    however it is slower.  Use this routine if you don't have handy a pointer
    to the last NBL in the chain.

Arguments:

    CountedQueue

    NblChain

--*/
{
    SIZE_T Count;
    NET_BUFFER_LIST *const LastNbl = NdisLastNblInNblChainWithCount(NblChain, &Count);

    // Overflow check
    NDIS_ASSERT(CountedQueue->NblCount + Count >= Count);

    NdisAppendNblChainToNblQueueFast(&CountedQueue->Queue, NblChain, LastNbl);

    CountedQueue->NblCount += Count;

    NDIS_ASSERT_VALID_NBL_COUNTED_QUEUE(CountedQueue);
}

_IRQL_requires_max_(DISPATCH_LEVEL)
inline
void
NdisAppendSingleNblToNblQueue(
    _In_ NBL_QUEUE *NblQueue,
    _In_ NET_BUFFER_LIST *Nbl)
/*++

Routine Description:

    Appends one NBL to an NBL_QUEUE

Arguments:

    NblQueue

    Nbl

--*/
{
    Nbl->Next = NULL;
    NdisAppendNblChainToNblQueueFast(NblQueue, Nbl, Nbl);
}

_IRQL_requires_max_(DISPATCH_LEVEL)
inline
void
NdisAppendSingleNblToNblCountedQueue(
    _In_ NBL_COUNTED_QUEUE *CountedQueue,
    _In_ NET_BUFFER_LIST *Nbl)
/*++

Routine Description:

    Appends one NBL to an NBL_COUNTED_QUEUE

Arguments:

    CountedQueue

    Nbl

--*/
{
    Nbl->Next = NULL;
    NdisAppendNblChainToNblCountedQueueFast(CountedQueue, Nbl, Nbl, 1);
}

_IRQL_requires_max_(DISPATCH_LEVEL)
inline
NET_BUFFER_LIST *
NdisPopAllFromNblQueue(
    _In_ NBL_QUEUE *NblQueue)
/*++

Routine Description:

    Removes all NBLs from the queue, and returns them in a chain

Arguments:

    NblQueue

Return Value:

    The previous contents of the NBL_QUEUE

--*/
{
    NET_BUFFER_LIST *const Nbls = NblQueue->First;

    NDIS_ASSERT_VALID_NBL_QUEUE(NblQueue);
    NdisInitializeNblQueue(NblQueue);

    return Nbls;
}

_IRQL_requires_max_(DISPATCH_LEVEL)
inline
NET_BUFFER_LIST *
NdisPopAllFromNblCountedQueue(
    _In_ NBL_COUNTED_QUEUE *CountedQueue)
/*++

Routine Description:

    Removes all NBLs from the queue, and returns them in a chain

Arguments:

    CountedQueue

Return Value:

    The previous contents of the NBL_COUNTED_QUEUE

--*/
{
    NET_BUFFER_LIST *const Nbls = CountedQueue->Queue.First;

    NDIS_ASSERT_VALID_NBL_COUNTED_QUEUE(CountedQueue);
    NdisInitializeNblCountedQueue(CountedQueue);

    return Nbls;
}

_IRQL_requires_max_(DISPATCH_LEVEL)
inline
NET_BUFFER_LIST *
NdisPopFirstNblFromNblQueue(
    _In_ NBL_QUEUE *NblQueue)
/*++

Routine Description:

    Removes the first NBL from the queue, if any

Arguments:

    NblQueue

Return Value:

    NULL if the NblQueue was empty, else
    the NBL that was recently the first NBL in the queue

--*/
{
    NDIS_ASSERT_VALID_NBL_QUEUE(NblQueue);

    if (NblQueue->First == NULL)
    {
        // The queue has 0 items
        return NULL;
    }
    else if (NblQueue->Last == &NblQueue->First->Next)
    {
        // The queue has 1 item
        return NdisPopAllFromNblQueue(NblQueue);
    }
    else
    {
        // The queue has 2+ items
        NET_BUFFER_LIST *const First = NblQueue->First;
        NblQueue->First = First->Next;
        First->Next = NULL;

        NDIS_ASSERT_VALID_NBL_QUEUE(NblQueue);
        return First;
    }
}

_IRQL_requires_max_(DISPATCH_LEVEL)
inline
NET_BUFFER_LIST *
NdisPopFirstNblFromNblCountedQueue(
    _In_ NBL_COUNTED_QUEUE *CountedQueue)
/*++

Routine Description:

    Removes the first NBL from the queue, if any

Arguments:

    CountedQueue

Return Value:

    NULL if the CountedQueue was empty, else
    the NBL that was recently the first NBL in the queue

--*/
{
    NET_BUFFER_LIST *Nbl;

    NDIS_ASSERT_VALID_NBL_COUNTED_QUEUE(CountedQueue);

    Nbl = NdisPopFirstNblFromNblQueue(&CountedQueue->Queue);

    if (Nbl != NULL)
    {
        CountedQueue->NblCount -= 1;
    }

    NDIS_ASSERT_VALID_NBL_COUNTED_QUEUE(CountedQueue);

    return Nbl;
}

#ifdef __cplusplus

inline NBL_QUEUE_t::NBL_QUEUE_t(NBL_QUEUE_t &&rhs)
{
    NDIS_ASSERT_VALID_NBL_QUEUE(&rhs);

    if (rhs.First)
    {
        this->First = rhs.First;
        this->Last = rhs.Last;
        NdisInitializeNblQueue(&rhs);
    }
    else
    {
        NdisInitializeNblQueue(this);
    }

    NDIS_ASSERT_VALID_NBL_QUEUE(this);
    NDIS_ASSERT_VALID_NBL_QUEUE(&rhs);
}

inline NBL_QUEUE_t &
NBL_QUEUE_t::operator=(NBL_QUEUE_t &&rhs)
{
    NDIS_ASSERT_VALID_NBL_QUEUE(&rhs);

    if (this->First != rhs.First)
    {
        if (rhs.First)
        {
            this->First = rhs.First;
            this->Last = rhs.Last;
            NdisInitializeNblQueue(&rhs);
        }
        else
        {
            NdisInitializeNblQueue(this);
        }
    }

    NDIS_ASSERT_VALID_NBL_QUEUE(this);
    NDIS_ASSERT_VALID_NBL_QUEUE(&rhs);
    return *this;
}

inline NBL_COUNTED_QUEUE_t::NBL_COUNTED_QUEUE_t(NBL_COUNTED_QUEUE_t &&rhs)
{
    NDIS_ASSERT_VALID_NBL_COUNTED_QUEUE(&rhs);

    if (rhs.Queue.First)
    {
        this->Queue.First = rhs.Queue.First;
        this->Queue.Last = rhs.Queue.Last;
        this->NblCount = rhs.NblCount;
        NdisInitializeNblCountedQueue(&rhs);
    }
    else
    {
        NdisInitializeNblCountedQueue(this);
    }

    NDIS_ASSERT_VALID_NBL_COUNTED_QUEUE(this);
    NDIS_ASSERT_VALID_NBL_COUNTED_QUEUE(&rhs);
}

inline NBL_COUNTED_QUEUE_t &
NBL_COUNTED_QUEUE_t::operator=(NBL_COUNTED_QUEUE_t &&rhs)
{
    NDIS_ASSERT_VALID_NBL_COUNTED_QUEUE(&rhs);

    if (this->Queue.First != rhs.Queue.First)
    {
        if (rhs.Queue.First)
        {
            this->Queue.First = rhs.Queue.First;
            this->Queue.Last = rhs.Queue.Last;
            this->NblCount = rhs.NblCount;
            NdisInitializeNblCountedQueue(&rhs);
        }
        else
        {
            NdisInitializeNblCountedQueue(this);
        }
    }

    NDIS_ASSERT_VALID_NBL_COUNTED_QUEUE(this);
    NDIS_ASSERT_VALID_NBL_COUNTED_QUEUE(&rhs);
    return *this;
}

namespace ndis {

struct nbl_queue : public NBL_QUEUE
{
    nbl_queue() { NdisInitializeNblQueue(this); }
    ~nbl_queue() = default;

    explicit nbl_queue(NET_BUFFER_LIST *nblChain)
    {
        this->First = nblChain;
        this->Last = &NdisLastNblInNblChain(nblChain)->Next;
        ASSERT_VALID();
    }

    nbl_queue(nbl_queue &) = delete;
    nbl_queue &operator=(nbl_queue &) = delete;

    nbl_queue(nbl_queue &&rhs) = default;
    nbl_queue(NBL_QUEUE &&rhs) : NBL_QUEUE(static_cast<NBL_QUEUE &&>(rhs)) {}

    nbl_queue &operator=(nbl_queue &&rhs) = default;
    nbl_queue &operator=(NBL_QUEUE &&rhs)
    {
        *static_cast<NBL_QUEUE *>(this) = static_cast<NBL_QUEUE &&>(rhs);
        return *this;
    }

    bool empty() const { return !!NdisIsNblQueueEmpty(this); }

    NET_BUFFER_LIST *get_nbls() const { return NdisGetNblChainFromNblQueue(this); }

    void append(_In_ NET_BUFFER_LIST *first, _In_ NET_BUFFER_LIST *last)
    {
        NdisAppendNblChainToNblQueueFast(this, first, last);
    }

    void append(_Inout_ NBL_QUEUE *queue) { NdisAppendNblQueueToNblQueueFast(this, queue); }

    void append_slow(_In_ NET_BUFFER_LIST *nblChain) { NdisAppendNblChainToNblQueue(this, nblChain); }

    void append_one_nbl(_In_ NET_BUFFER_LIST *nbl) { NdisAppendSingleNblToNblQueue(this, nbl); }

    NET_BUFFER_LIST *clear() { return NdisPopAllFromNblQueue(this); }

    NET_BUFFER_LIST *pop() { return NdisPopFirstNblFromNblQueue(this); }

    void ASSERT_VALID() const { NDIS_ASSERT_VALID_NBL_QUEUE(this); }
};

struct nbl_counted_queue : public NBL_COUNTED_QUEUE
{
    nbl_counted_queue() { NdisInitializeNblCountedQueue(this); }

    ~nbl_counted_queue() = default;

    explicit nbl_counted_queue(NET_BUFFER_LIST *nblChain)
    {
        SIZE_T count;
        auto last = NdisLastNblInNblChainWithCount(nblChain, &count);

        this->Queue.First = nblChain;
        this->Queue.Last = &last->Next;
        this->NblCount = count;

        ASSERT_VALID();
    }

    nbl_counted_queue(nbl_counted_queue &&rhs) = default;
    nbl_counted_queue(NBL_COUNTED_QUEUE &&rhs) : NBL_COUNTED_QUEUE(static_cast<NBL_COUNTED_QUEUE &&>(rhs)) {}

    nbl_counted_queue &operator=(nbl_counted_queue &&rhs) = default;
    nbl_counted_queue &operator=(NBL_COUNTED_QUEUE &&rhs)
    {
        *static_cast<NBL_COUNTED_QUEUE *>(this) = static_cast<NBL_COUNTED_QUEUE &&>(rhs);
        return *this;
    }

    nbl_counted_queue(nbl_counted_queue &) = delete;
    nbl_counted_queue &operator=(nbl_counted_queue &) = delete;

    bool empty() const { return !!NdisIsNblCountedQueueEmpty(this); }

    NET_BUFFER_LIST *get_nbls() const { return NdisGetNblChainFromNblCountedQueue(this); }

    size_t count() const { return this->NblCount; }

    void append(_In_ NET_BUFFER_LIST *first, _In_ NET_BUFFER_LIST *last, _In_ size_t numNbls)
    {
        NdisAppendNblChainToNblCountedQueueFast(this, first, last, numNbls);
    }

    void append(_Inout_ NBL_COUNTED_QUEUE *queue) { NdisAppendNblCountedQueueToNblCountedQueueFast(this, queue); }

    void append_slow(_In_ NET_BUFFER_LIST *nblChain) { NdisAppendNblChainToNblCountedQueue(this, nblChain); }

    void append_one_nbl(_In_ NET_BUFFER_LIST *nbl) { NdisAppendSingleNblToNblCountedQueue(this, nbl); }

    NET_BUFFER_LIST *clear() { return NdisPopAllFromNblCountedQueue(this); }

    NET_BUFFER_LIST *pop() { return NdisPopFirstNblFromNblCountedQueue(this); }

    void ASSERT_VALID() const { NDIS_ASSERT_VALID_NBL_COUNTED_QUEUE(this); }
};

namespace details
{

//
// These exist only to make range-based for loop work; don't use them directly.
// Instead, you can use them indirectly like this:
//
//      ndis::nbl_queue queue = . . .;
//      for (auto &&nbl : queue) {
//          DoSomething(nbl);
//      }
//
class nbl_queue_iterator
{
public:
    nbl_queue_iterator() : p{nullptr} {}
    explicit nbl_queue_iterator(ndis::nbl_queue const &q) : p{q.First} {}

    NET_BUFFER_LIST *operator++() { p = p->Next; return p; }
    NET_BUFFER_LIST *operator*() { return p; }
    bool operator!=(nbl_queue_iterator &rhs) { return p != rhs.p; }

private:
    NET_BUFFER_LIST *p;
};

} // namespace details

inline auto begin(nbl_queue const &q) { return ndis::details::nbl_queue_iterator{q}; }
inline auto end(nbl_queue const &) { return ndis::details::nbl_queue_iterator{}; }

} // namespace ndis

#endif // __cplusplus

#endif // WINAPI_FAMILY_PARTITION(WINAPI_PARTITION_SYSTEM | WINAPI_PARTITION_DESKTOP)
#pragma endregion
