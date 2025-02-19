﻿using System;
using System.Collections.Generic;
using Domain.Events;

namespace Domain.Entities
{
    public class Bbq : AggregateRoot
    {
        public string Reason { get; set; }
        public BbqStatus Status { get; set; }
        public DateTime Date { get; set; }
        public bool IsTrincasPaying { get; set; }
        public int AcceptCount { get; set; }
        public Dictionary<string, int> ShoppingList { get; set; }
        public const int VEGAN_VEGETABLE_COUNT = 600;
        public const int NORMAL_VEGETABLE_COUNT = 300;
        public const int NORMAL_MEAT_COUNT = 300;
        private void When(ThereIsSomeoneElseInTheMood @event)
        {
            Id = @event.Id.ToString();
            Date = @event.Date;
            Reason = @event.Reason;
            Status = BbqStatus.New;
            AcceptCount = 0;
            ShoppingList = new Dictionary<string, int> {
                { "Meat", 0},
                { "Vegetables", 0}
            };
        }

        private void When(BbqStatusUpdated @event)
        {
            if (@event.GonnaHappen)
                Status = BbqStatus.PendingConfirmations;
            else 
                Status = BbqStatus.ItsNotGonnaHappen;

            if (@event.TrincaWillPay)
                IsTrincasPaying = true;
        }

        private void When(InviteWasAccepted @event)
        {
            AcceptCount++;
            if (AcceptCount >= 7)
                Status = BbqStatus.Confirmed;

            if (@event.IsVeg)
                ShoppingList["Vegetables"] += VEGAN_VEGETABLE_COUNT;
            else
            {
                ShoppingList["Vegetables"] += NORMAL_VEGETABLE_COUNT;
                ShoppingList["Meat"] += NORMAL_MEAT_COUNT;
            }
        }

        private void When(InviteWasDeclined @event)
        {
            AcceptCount--;
            if (AcceptCount < 7)
                Status = BbqStatus.PendingConfirmations;

            if (@event.IsVeg)
                ShoppingList["Vegetables"] -= VEGAN_VEGETABLE_COUNT;
            else
            {
                ShoppingList["Vegetables"] -= NORMAL_VEGETABLE_COUNT;
                ShoppingList["Meat"] -= NORMAL_MEAT_COUNT;
            }
        }

        public object TakeSnapshot()
        {
            return new
            {
                Id,
                Date,
                IsTrincasPaying,
                Status = Status.ToString()
            };
        }
    }
}
