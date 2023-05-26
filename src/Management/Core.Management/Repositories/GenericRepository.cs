using System;
using System.Net;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Linq.Dynamic.Core;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

using Core.Domain.Exceptions;
using Core.Management.Interfaces;

using KVP = System.Collections.Generic.KeyValuePair<string, string>;

namespace Core.Management.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {

        private LinkedList<KVP> propertyNamesList = new LinkedList<KVP>();
        private readonly string entityName = typeof(TEntity).Name;
        protected readonly DbContext context;
        protected readonly DbSet<TEntity> dbSet;

        public GenericRepository(DbContext context)
        {
            this.context = context;
            dbSet = context.Set<TEntity>();
        }

        public IEnumerable<TEntity> GetAll(string orderByExpression, out int totalCount, int pageSize = 1, int startIndex = 0)
        {
            totalCount = dbSet.Count();
            return dbSet
                .OrderBy(orderByExpression)
                .Skip(startIndex)
                .Take(pageSize);
        }

        /// <summary>
        /// Eager-load entities to avoid multiple trips to the database when iterating over collections and want to control specifically which related entities and collections are retrieved by
        /// telling the Repository which to include in the re-hydrated object graph.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="orderBy"></param>
        /// <param name="includeExpressions"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> Fetch(Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, params Expression<Func<TEntity, object>>[] includeExpressions)
        {
            IQueryable<TEntity> query = predicate is null ? dbSet : dbSet.Where(predicate);

            if (includeExpressions.Any())
            {
                query = includeExpressions.Aggregate(query, (current, expression) => current.Include(expression));
                return query.Where(predicate);
            }

            return orderBy is null ? query : orderBy(query);
        }

        /// <summary>
        /// Eager-load entities to avoid multiple trips to the database when iterating over collections and want to control specifically which related entities and collections are retrieved by
        /// telling the Repository which to include in the re-hydrated object graph.
        /// Use: Fetch(x => x.UserId == 1, includeExpressions: new Expression<Func<User, object>>[] { x => x.UserAccounts, x => x.UserRoles }); || Fetch(x => x.UserId == 1, x => x.UserAccounts, x => x.UserRoles);      
        /// </summary>
        /// <param name="predicate">Functional construct for asserting a given TEntity object</param>
        /// <param name="includeExpressions">Include Properties</param>
        /// <returns>Return TEntity with it's related entities and collections</returns>
        public IEnumerable<TEntity> Fetch(Func<TEntity, bool> predicate, params Expression<Func<TEntity, object>>[] includeExpressions)
        {
            if (includeExpressions.Any())
            {
                IQueryable<TEntity> set = includeExpressions.Aggregate<Expression<Func<TEntity, object>>, IQueryable<TEntity>>(dbSet, (current, expression) => current.Include(expression));
                return set.Where(predicate);
            }

            return dbSet.Where(predicate);
        }

        public IEnumerable<TEntity> Search(Expression<Func<TEntity, bool>> predicate, string orderByExpression, int pageSize = 1, int startIndex = 0)
        {
            return dbSet.Where(predicate).OrderBy(orderByExpression).Skip(startIndex).Take(pageSize);
        }

        public IEnumerable<TEntity> Search(Expression<Func<TEntity, bool>> predicate, string orderByExpression, out int totalCount, int pageSize = 1, int startIndex = 0)
        {
            totalCount = dbSet
                .Where(predicate)
                .Count();

            return dbSet
                .Where(predicate)
                .OrderBy(orderByExpression)
                .Skip(startIndex)
                .Take(pageSize);
        }

        /// <summary>
        /// Gets the TEntity by the predicate from the database
        /// </summary>
        /// <param name="predicate">Binary expression defining the criteria by which to get the TEntity</param>
        /// <param name="checkActive">Checks if TEntity is active. Returns null or throws exception depending on <paramref name="throwException"/> </param>
        /// <param name="activePropertyName">The name of the property to check. Provides for different names, e.g IsActive, IsEnabled, e.t.c</param>
        /// <param name="throwException">True: Handle invalid status. False: Ignore invalid status and assume caller will handle it</param>
        /// <param name="inverseCheck">True: If TEntity exists throws exception or return TEntity <paramref name="throwException"/>. False: Ignore that TEntity exists</param>
        /// <returns>Returns TEntity</returns>
        public async Task<TEntity> ValidatedSearch(Expression<Func<TEntity, bool>> predicate, bool checkActive = false, string activePropertyName = "IsActive", bool throwException = true, bool inverseCheck = false, params Expression<Func<TEntity, object>>[] includeExpressions)
        {
            propertyNamesList = new LinkedList<KVP>();

            GetPropertyName(predicate.Body);

            TEntity entity = includeExpressions.Length < 1 ? await dbSet.FirstOrDefaultAsync(predicate).ConfigureAwait(false) :
                await includeExpressions.Aggregate<Expression<Func<TEntity, object>>, IQueryable<TEntity>>(dbSet, (current, expression) => current.Include(expression))
                .FirstOrDefaultAsync(predicate).ConfigureAwait(false);

            if (entity != null && inverseCheck)
                throw new GenericException($"{GetPropertiesString()} is already registered", "AN002", HttpStatusCode.Conflict);

            if (entity == null && !inverseCheck)
            {
                if (throwException)
                    throw new GenericException($"{entityName} with {GetPropertiesString()} could not be found", "AN001", HttpStatusCode.NotFound);

                return null;
            }

            if (checkActive && entity != null)
            {
                PropertyInfo property = typeof(TEntity).GetProperty(activePropertyName);
                if (property != null)
                {
                    if (property.GetValue(entity).ToString().ToUpper() != bool.TrueString.ToUpper())
                    {
                        if (throwException)
                            throw new GenericException($"{entityName} with {GetPropertiesString()} is deactivated", "AN003", HttpStatusCode.Gone);
                        return null;
                    }
                }
            }

            return entity;
        }

        /// <summary>
        /// Gets the user by their username from the database
        /// </summary>
        /// <param name="username">The username of the user to retrieve from the database</param>
        /// <param name="checkActive">Checks if user is active. Returns null or throws exception depending on <paramref name="throwException"/> </param>
        /// <param name="throwException">True: Handle invalid status. False: Ignore invalid status and assume caller will handle it</param>
        /// <param name="inverseCheck">True: If user exists throws exception or return user <paramref name="throwException"/>. False: Ignore that the user exists</param>
        /// <returns>Returns TEntity</returns>
        public async Task<TEntity> ValidatedFind(Expression<Func<TEntity, bool>> predicate, bool throwException = true, bool inverseCheck = false, params Expression<Func<TEntity, object>>[] includeExpressions)
        {
            propertyNamesList = new LinkedList<KVP>();

            GetPropertyName(predicate.Body);

            TEntity entity = includeExpressions.Length < 1 ? await dbSet.FirstOrDefaultAsync(predicate).ConfigureAwait(false) :
                await includeExpressions.Aggregate<Expression<Func<TEntity, object>>, IQueryable<TEntity>>(dbSet, (current, expression) => current.Include(expression))
                .FirstOrDefaultAsync(predicate).ConfigureAwait(false);

            if (entity != null && inverseCheck)
                throw new GenericException($"{GetPropertiesString()} is already registered", "AN002", HttpStatusCode.Conflict);

            if (entity is null && throwException)
                throw new GenericException($"{entityName} with {GetPropertiesString()} could not be found", "AN001", HttpStatusCode.NotFound);

            return entity;
        }

        /// <summary>
        /// Peeks for entity from the database
        /// </summary>
        /// <param name="throwException">True: Handle null entity. False: Ignore null entity and assume caller will handle it</param>
        /// <param name="inverseCheck">True: If entity exists throws exception or return value <paramref name="throwException"/>. False: Ignore that the entity exists</param>
        /// <returns>Returns bool</returns>
        public async Task<bool> ValidatedAny(Expression<Func<TEntity, bool>> predicate, bool throwException = false, bool inverseCheck = true)
        {
            propertyNamesList = new LinkedList<KVP>();

            GetPropertyName(predicate.Body);

            bool exists = await dbSet.AsNoTracking().AnyAsync(predicate).ConfigureAwait(false);

            if (exists && inverseCheck)
                throw new GenericException($"{GetPropertiesString()} is already registered", "AN002", HttpStatusCode.Conflict);

            if (!exists && throwException)
                throw new GenericException($"{entityName} with {GetPropertiesString()} could not be found", "AN001", HttpStatusCode.NotFound);

            return exists;
        }

        /// <summary>
        /// Activates|Enables or deactivates|disables an entity.
        /// </summary>
        /// <param name="predicate">Criteria fetch the entity from the data store</param>
        /// <param name="active">Sets whether to activate or deactivate the entity</param>
        /// <param name="propertyName">The name of the property to check. Provides for different names, e.g IsActive, IsEnabled e.t.c</param>
        /// <returns>Entity whose active status has been changed</returns>
        public async Task<TEntity> ToggleActiveStatus(Expression<Func<TEntity, bool>> predicate, bool active, string propertyName = "IsActive")
        {
            TEntity entity = await dbSet.FirstOrDefaultAsync(predicate).ConfigureAwait(false);

            if (entity is null) throw new GenericException($"{entityName} with {GetPropertiesString()} could not be found", "AN001", HttpStatusCode.NotFound);

            PropertyInfo property = typeof(TEntity).GetProperty(propertyName);

            if (property == null)
            {
                throw new GenericException($"{entityName} does not have provision for '{propertyName}' property", "AN004", HttpStatusCode.Forbidden);
            }
            if ((bool)property.GetValue(entity) == active) return entity;

            property.SetValue(entity, active);

            return entity;
        }

        private KVP GetPropertyName<TSource, TProperty>(TSource source, Expression<Func<TSource, TProperty>> propertyLambda)
        {
            GetPropertyName(propertyLambda, out string propertyName, out string propertyValue);

            return new KVP(propertyName, propertyValue);
        }

        private void GetPropertyName<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda, out string propertyName, out string propertyValue)
        {
            BinaryExpression expression = (BinaryExpression)propertyLambda.Body;
            MemberInfo member = ((MemberExpression)expression.Left).Member;
            string name = member.Name;
            Expression value = expression.Right;


            if (member.IsDefined(typeof(DisplayNameAttribute)))
            {
                Attribute displayNameAttribute = member.GetCustomAttribute(typeof(DisplayNameAttribute));
                name = ((DisplayNameAttribute)displayNameAttribute).DisplayName;
            }

            propertyName = name;
            propertyValue = value.ToString();
        }

        private void GetPropertyName(Expression expressionParameter)
        {
            string name;
            string value;
            MemberInfo member;

            if (expressionParameter.GetType() == typeof(UnaryExpression))
            {
                UnaryExpression expression = (UnaryExpression)expressionParameter;
                member = ((MemberExpression)expression.Operand).Member;
                name = member.Name;
                value = expression.NodeType.ToString();
            }
            else
            {
                BinaryExpression expression = (BinaryExpression)expressionParameter;

                if (expression.Right.GetType() != typeof(ConstantExpression)
                    && expression.Right.GetType() != typeof(MemberExpression)
                    && !expression.Right.GetType().GetTypeInfo().IsSubclassOf(typeof(MemberExpression)))
                {
                    GetPropertyName(expression.Left);
                    GetPropertyName(expression.Right);

                    return;
                }
                member = ((MemberExpression)expression.Left).Member;
                name = member.Name;

                //get value
                object fieldValue = ExpressionUtilities.GetValueWithoutCompiling(expression.Right);
                value = fieldValue.ToString();
            }

            if (member.IsDefined(typeof(DisplayNameAttribute)))
            {
                Attribute displayNameAttribute = member.GetCustomAttribute(typeof(DisplayNameAttribute));

                name = ((DisplayNameAttribute)displayNameAttribute).DisplayName;
            }

            propertyNamesList.AddLast(new KVP(name, value));
        }

        private string GetPropertiesString()
        {

            string message = "";
            string seperator = propertyNamesList.Count > 1 ? "| " : "";

            foreach (KVP item in propertyNamesList)
            {
                message += $"{item.Key} {item.Value} ";

                if (!propertyNamesList.Last.Value.Equals(item)) message += seperator;
            }

            return message;
        }

    }

}
