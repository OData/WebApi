using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ODataSample.Web.Controllers
{
	public static class CrudBaseExtensions
	{
		public static void EnsureEntity<T, TKey>(this CrudBase<T, TKey> crud, TKey id, Action<T> update)
			where T : class
		{
			var entity = crud.Find(id);
			if (entity == null)
			{
				entity = Activator.CreateInstance<T>();
				crud.SetEntityId(entity, id);
				crud.Add(entity);
			}
			update(entity);
		}
	}

	public class CrudBase<T, TKey>
		where T : class
	{
		private readonly DbContext _context;
		private readonly DbSet<T> _entities;

		//public static DbSet<T> FindDbSet(DbContext dataContext)
		//{
		//	var propertyInfo = dataContext.GetType().GetTypeInfo().GetRuntimeProperties().FirstOrDefault();
		//	if (propertyInfo != null)
		//		return (DbSet<T>)propertyInfo.GetValue(dataContext);
		//	throw new InvalidOperationException("Cannot find dbset for type {0}".FormatText(typeof(T)));
		//}

		public CrudBase(DbContext context, DbSet<T> entities, Expression<Func<T, TKey>> idPropertyExpression)
		{
			IdProperty = (idPropertyExpression.Body as MemberExpression).Member as PropertyInfo;
			_context = context;
			_entities = entities;
		}

		public PropertyInfo IdProperty { get; set; }

		public IQueryable<T> All()
		{
			return _entities;
		}

		public T Find(TKey id)
		{
			return _entities.SingleOrDefault(KeyEqualsExpression(id));
		}
		public IQueryable<T> FindQuery(TKey id)
		{
			return _entities.Where(KeyEqualsExpression(id));
		}

		private Expression<Func<T, bool>> KeyEqualsExpression(TKey key)
		{
			// TODO: Blog about needing a parameter name here, looks like a bug
			var parameterExpression = Expression.Parameter(typeof(T), "entity");
			var propertyName = IdProperty.Name;
			//if (typeof (IDbObject<TKey>).GetTypeInfo().IsAssignableFrom(typeof (T).GetTypeInfo()))
			//{
			//    propertyName = nameof(IDbObject<TKey>.Id);
			//}
			if (string.IsNullOrEmpty(propertyName))
				throw new NotSupportedException();
			return Expression.Lambda<Func<T, bool>>(
				Expression.Equal(Expression.Property(parameterExpression, propertyName), Expression.Constant(key)),
				parameterExpression);
		}

		public T Add(T entity)
		{
			var existingProduct = Find(EntityId(entity));
			if (existingProduct != null)
			{
				return existingProduct;
			}

			_entities.Add(entity);
			return entity;
		}

		public async Task<T> AddAndSaveAsync(T entity)
		{
			var rtn = Add(entity);
			await _context.SaveChangesAsync();
			return rtn;
		}

		public TKey EntityId(T entity)
		{
			return (TKey)IdProperty.GetValue(entity);
		}

		public void SetEntityId(T entity, TKey id)
		{
			IdProperty.SetValue(entity, id);
		}

		public bool Update(T product)
		{
			_entities.Update(product);
			return true;
		}

		public async Task<bool> UpdateAndSaveAsync(T product)
		{
			var rtn = Update(product);
			await _context.SaveChangesAsync();
			return rtn;
		}

		public bool Delete(TKey id)
		{
			var entity = _entities.FirstOrDefault(KeyEqualsExpression(id));
			if (entity != null)
			{
				_entities.Remove(entity);
			}
			return entity != null;
		}

		public async Task<bool> DeleteAndSaveAsync(TKey id)
		{
			var rtn = Delete(id);
			await _context.SaveChangesAsync();
			return rtn;
		}

		public async Task SaveAsync()
		{
			await _context.SaveChangesAsync();
		}
	}
}