using System;
using System.IO;

namespace Albatross.Tools
{
    /// <summary>
    /// 파일 소스 추상화 인터페이스 - 메모리 내 데이터와 디스크 세그먼트를 통일된 방식으로 처리
    /// (.NET Framework 4.7.2 호환 버전 - byte[] 배열 사용)
    /// </summary>
    public interface IFileSource : IDisposable
    {
        /// <summary>파일 크기 (바이트)</summary>
        long Length { get; }

        /// <summary>
        /// 스트림 간 직접 복사 (버퍼 재사용)
        /// </summary>
        /// <param name="destination">목적지 스트림</param>
        /// <param name="copyBuffer">재사용 버퍼</param>
        void CopyTo(Stream destination, byte[] copyBuffer);

        /// <summary>
        /// 레거시 API 호환용 - 전체 데이터를 byte 배열로 반환
        /// </summary>
        byte[] ToArray();
    }

    /// <summary>
    /// 메모리 내 byte[] 데이터를 래핑하는 FileSource
    /// </summary>
    public sealed class MemorySource : IFileSource
    {
        private byte[] _data;

        public long Length => _data?.Length ?? 0;

        public MemorySource(byte[] data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public void CopyTo(Stream destination, byte[] copyBuffer)
        {
            if (_data == null || _data.Length == 0) return;
            destination.Write(_data, 0, _data.Length);
        }

        public byte[] ToArray()
        {
            return _data;
        }

        public void Dispose()
        {
            // 메모리는 GC가 관리하므로 명시적 해제 불필요
            _data = null;
        }
    }

    /// <summary>
    /// 디스크 파일의 특정 세그먼트를 가리키는 FileSource (지연 로딩)
    /// </summary>
    public sealed class FileSegmentSource : IFileSource
    {
        private readonly string _filePath;
        private readonly long _offset;
        private readonly long _length;

        public long Length => _length;

        public FileSegmentSource(string filePath, long offset, long length)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            _filePath = filePath;
            _offset = offset;
            _length = length;
        }

        public void CopyTo(Stream destination, byte[] copyBuffer)
        {
            using (var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                fs.Seek(_offset, SeekOrigin.Begin);
                long remaining = _length;

                while (remaining > 0)
                {
                    int toRead = (int)Math.Min(remaining, copyBuffer.Length);
                    int read = fs.Read(copyBuffer, 0, toRead);
                    if (read == 0) break;

                    destination.Write(copyBuffer, 0, read);
                    remaining -= read;
                }
            }
        }

        public byte[] ToArray()
        {
            byte[] buffer = new byte[_length];
            using (var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                fs.Seek(_offset, SeekOrigin.Begin);
                int totalRead = 0;
                while (totalRead < _length)
                {
                    int read = fs.Read(buffer, totalRead, (int)(_length - totalRead));
                    if (read == 0) break;
                    totalRead += read;
                }
            }
            return buffer;
        }

        public void Dispose()
        {
            // 파일 경로만 저장하므로 해제할 리소스 없음
        }
    }
}
