using serena.Entities;
using serena.DAL;

namespace serena.BLL
{
    public class MemberBLL
    {
        private MemberDAL _dal = new MemberDAL();

        public Member Login(string username, string password)
        {
            var member = _dal.GetByUsername(username);
            if (member != null && member.Password == password) // In a real app, use hashing!
            {
                return member;
            }
            return null;
        }

        public bool Register(Member m)
        {
            if (_dal.GetByUsername(m.Username) != null) return false;
            return _dal.Insert(m) > 0;
        }

        public Member GetMember(int id)
        {
            return _dal.GetById(id);
        }
    }
}
