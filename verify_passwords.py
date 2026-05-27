import bcrypt

admin_hash = b"$2a$11$VyGwoqxHC6gMQ9iMsda/7eE9a5TV9SOHBRyX4SgwU.RJNNxnYEera"
lawyer_hash = b"$2a$11$CnI9Ur82n8LPzJkcFCD6Q.D4J892KK5RHTh7BAXnHCmKE3cQOxOey"

print("admin123 vs admin_hash:", bcrypt.checkpw(b"admin123", admin_hash))
print("lawyer123 vs lawyer_hash:", bcrypt.checkpw(b"lawyer123", lawyer_hash))
