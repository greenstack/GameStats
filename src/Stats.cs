/*
 * Copyright © 2023 Greenstack
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the “Software”), to
 * deal in the Software without restriction, including without limitation the
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
 * sell copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
 * IN THE SOFTWARE.
 */

using System.Numerics;

namespace Greenstack.GameStats
{
	// TODO: Create a base game stat class
	// TODO: Create unit tests for these stats

	#region Delegates
	public delegate void StatValueChanged<T>(IStat<T> newValue, T oldValue) where T : INumber<T>;
	#endregion

	#region Interfaces
	/// <summary>
	/// Basic interface for a statistic.
	/// </summary>
	/// <typeparam name="T">The underlying type for this stat.</typeparam>
	public interface IStat<T>
		where T : INumber<T>
	{
		/// <summary>
		/// The current value of this stat.
		/// </summary>
		T CurrentValue { get; }

		public static bool operator >(IStat<T> lhs, IStat<T> rhs) =>
			lhs.CurrentValue > rhs.CurrentValue;

		public static bool operator <(IStat<T> lhs, IStat<T> rhs) =>
			lhs.CurrentValue < rhs.CurrentValue;

		public static bool operator >=(IStat<T> lhs, IStat<T> rhs) =>
			lhs.CurrentValue >= rhs.CurrentValue;

		public static bool operator <=(IStat<T> lhs, IStat<T> rhs) =>
			lhs.CurrentValue <= rhs.CurrentValue;

		public static T operator *(T lhs, IStat<T> rhs) =>
			lhs * rhs.CurrentValue;

		public static T operator *(IStat<T> lhs, T rhs) =>
			lhs.CurrentValue * lhs;

		event StatValueChanged<T> OnStatChanged;
	}

	/// <summary>
	/// A stat with a lower bound.
	/// </summary>
	/// <typeparam name="T">The underlying type of the stat.</typeparam>
	public interface ILowerBoundedStat<T> : IStat<T>
		where T : INumber<T>, IMinMaxValue<T>
	{
		/// <summary>
		/// The minimum value for this stat.
		/// </summary>
		T Min { get; }
	}

	
	/// <summary>
	/// A stat whose current value can be reduced.
	/// </summary>
	/// <typeparam name="T">The underlying type of the stat.</typeparam>
	public interface IReduceableStat<T> : IStat<T>
		where T : INumber<T>
	{
		/// <summary>
		/// Reduces the current value by the given amount.
		/// </summary>
		/// <param name="amount">The amount to lower the value by.</param>
		void Reduce(T amount);
	}

	public interface IDepleteableStat<T> : IReduceableStat<T>, ILowerBoundedStat<T>
		where T : INumber<T>, IMinMaxValue<T>
	{
		/// <summary>
		/// Reduces the stat to <see cref="ILowerBoundedStat{T}.Min"/>
		/// </summary>
		void Deplete();
	}

	/// <summary>
	/// A stat with an upper bound.
	/// </summary>
	/// <typeparam name="T">The underlying type of the stat.</typeparam>
	public interface IUpperBoundedStat<T> : IStat<T>
		where T : INumber<T>, IMinMaxValue<T>
	{
		/// <summary>
		/// The maximum value for this stat.
		/// </summary>
		T Max { get; }
	}

	/// <summary>
	/// A stat whose current value can be increased.
	/// </summary>
	/// <typeparam name="T">The underlying type of the stat.</typeparam>
	public interface IIncreasableStat<T> : IStat<T>
		where T : INumber<T>
	{
		/// <summary>
		/// Increases the current value of the stat by the given amount.
		/// </summary>
		/// <param name="amount">The amount to set the value to.</param>
		void Increase(T amount);
	}

	/// <summary>
	/// A stat whose value can be replenished.
	/// </summary>
	/// <typeparam name="T">The underlying type of the stat.</typeparam>
	public interface IReplenishableStat<T> : IIncreasableStat<T>, IUpperBoundedStat<T>
		where T : INumber<T>, IMinMaxValue<T>
	{
		/// <summary>
		/// Replenishes a portion of the stat by a given percentage.
		/// </summary>
		/// <param name="percent">The percentage by which to restore the stat.</param>
		void ReplenishPercentOfMax(float percent)
			=> Increase(T.CreateChecked(float.CreateChecked(Max) * percent));

		/// <summary>
		/// Replenishes a percentage of the current stat.
		/// </summary>
		/// <param name="percent">The percentage to restore.</param>
		void ReplenishPercentOfCurrent(float percent)
			=> Increase(T.CreateChecked(float.CreateChecked(CurrentValue) * percent));

		/// <summary>
		/// Sets the current value of the stat to max value.
		/// </summary>
		void RestoreInFull();
	}

	/// <summary>
	/// A stat with an upper and lower bound. The current value is always clamped.
	/// </summary>
	/// <typeparam name="T">The underlying type of the stat.</typeparam>
	public interface IClampedStat<T> : ILowerBoundedStat<T>, IUpperBoundedStat<T>
		where T : INumber<T>, IMinMaxValue<T>
	{
		// I wish I could at the "set" method in further interfaces but oh well
	}

	/// <summary>
	/// A stat whose current value can be reset to its base value.
	/// </summary>
	/// <typeparam name="T">The underlying type of the stat.</typeparam>
	public interface IResettableStat<T> : IStat<T>
		where T : INumber<T>
	{
		/// <summary>
		/// The base value for this stat.
		/// </summary>
		T BaseValue { get; }

		/// <summary>
		/// Resets this stat's CurrentValue to its BaseValue.
		/// </summary>
		void Reset();
	}

	public interface IResourceStat<T> : IClampedStat<T>, IReplenishableStat<T>, IDepleteableStat<T>
		where T : INumber<T>, IMinMaxValue<T>
	{

	}

	/// <summary>
	/// A resettable stat whose value can be modified in some way.
	/// </summary>
	/// <typeparam name="T">The underlying type of the stat.</typeparam>
	public interface IModifiableStat<T> : IResettableStat<T>
		where T : INumber<T>
	{
		void AddModifier(IStatModifier modifier);
	}
	#endregion Interfaces

	#region Modifier Interfaces
	/// <summary>
	/// The base interface for a modifier for various a <see cref="IModifiableStat{T}"/>.
	/// </summary>
	public interface IStatModifier
	{
		/// <summary>
		/// Is this modifier currently active?
		/// </summary>
		bool IsActive { get; }

		/// <summary>
		/// Applies this modifier to the stat.
		/// </summary>
		/// <typeparam name="T">The underlying type of the stat.</typeparam>
		/// <param name="value">The incoming value to be modified.</param>
		void Apply<T>(out T value) where T : INumber<T>;
	}

	/// <summary>
	/// A modifier for various stats.
	/// </summary>
	/// <typeparam name="T">The type of number that does the modification.</typeparam>
	public interface IStatModifier<T> : IStatModifier
		where T : INumber<T>
	{
		T ModificationValue { get; }
	}
	#endregion Modifier Interfaces

	#region Classes

	public class ResourceStat<T> : IResourceStat<T>
		where T : INumber<T>, IMinMaxValue<T>
	{
		public T Max { get; set; } = T.MaxValue;

		private T _currentValue;

		public event StatValueChanged<T>? OnStatChanged;

		public required T CurrentValue
		{
			get => _currentValue;
			set
			{
				T oldValue = _currentValue;
				_currentValue = T.Clamp(value, Min, Max);
				if (_currentValue != oldValue)
				{
					OnStatChanged?.Invoke(this, oldValue);
				}
			}
		}

		public T Min { get; set; } = T.Zero;

		/// <summary>
		/// Is this resource completely drained?
		/// </summary>
		public bool IsDepleted => CurrentValue == Min;

#pragma warning disable 8618 // CurrentValue is a setter for _currentValue
		/// <summary>
		/// 
		/// </summary>
		/// <param name="currentValue"></param>
		public ResourceStat()
#pragma warning restore 8618
		{
		}

		public void Increase(T amount)
		{
			CurrentValue += amount;
		}

		public void RestoreInFull()
		{
			CurrentValue = Max;
		}

		public void Deplete()
		{
			CurrentValue = Min;
		}

		public void Reduce(T amount)
		{
			CurrentValue -= amount;
		}
	}

	/// <summary>
	/// Represents a stat whose value can be reset to its base value.
	/// </summary>
	/// <typeparam name="T">The underlying type of this stat.</typeparam>
	public class ModifiableStat<T> : IModifiableStat<T>
		where T : INumber<T>
	{
		private T _currentValue;
		public T CurrentValue
		{
			get => _currentValue;
			protected set
			{
				if (_currentValue != value)
				{
					_currentValue = value;
					OnStatChanged?.Invoke(this, _currentValue);
				}
			}
		}

		private ICollection<IStatModifier> _modifiers = new List<IStatModifier>();

		public event StatValueChanged<T>? OnStatChanged;

		/// <summary>
		/// Constructor for ModifiableStat.
		/// </summary>
		/// <param name="baseValue">The base value of the stat. CurrentValue will be set to this as well.</param>
		public ModifiableStat(T baseValue)
		{
			BaseValue = _currentValue = baseValue;
		}

		/// <summary>
		/// Constructor for ModifiableStat.
		/// </summary>
		/// <param name="baseValue">The base value for this stat.</param>
		/// <param name="currentValue">The current value for the stat.</param>
		public ModifiableStat(T baseValue, T currentValue)
		{
			BaseValue = baseValue;
			CurrentValue = currentValue;
		}

		/// <summary>
		/// The base value for this stat.
		/// </summary>
		public T BaseValue { get; init; }

		public void AddModifier(IStatModifier modifier)
		{
			_modifiers.Add(modifier);

			T finalValue = BaseValue;

			foreach (var mod in _modifiers)
			{
				if (mod.IsActive)
					mod.Apply(out finalValue);
			}

			CurrentValue = finalValue;
		}

		/// <summary>
		/// Resets <see cref="IStat{T}.CurrentValue"/> to <see cref="BaseValue"/>
		/// </summary>
		public virtual void Reset()
		{
			CurrentValue = BaseValue;
			_modifiers.Clear();
		}
	}
	#endregion Classes

	// A section of interfaces that leverage C# interfaces.
	#region Helper Interfaces
	// TODO: Try to figure out how to declare my own number interface
	#endregion Helper Interfaces
}
