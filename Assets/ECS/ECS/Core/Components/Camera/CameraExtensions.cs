namespace ME.ECS {

    using UnityEngine;
    
    public static class CameraExtensions {

        public static Camera.Camera CreateCameraComponent(this UnityEngine.Camera camera) {

            var component = new ME.ECS.Camera.Camera {
                perspective = (camera.orthographic == false),
                fieldOfView = camera.fieldOfView,
                aspect = camera.aspect,
                orthoSize = camera.orthographicSize,
                farClipPlane = camera.farClipPlane,
                nearClipPlane = camera.nearClipPlane
            };
            return component;

        }
        
        /// <summary>
        /// UnityEngine.Camera::ViewportToWorldPoint
        /// </summary>
        /// <param name="entity">Entity with ME.ECS.Camera.Camera component</param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static UnityEngine.Vector3 ViewportToWorldPoint(this Entity entity, UnityEngine.Vector3 position) {

            if (entity.HasData<ME.ECS.Camera.Camera>() == false) return Vector3.zero;
            
            var camera = entity.GetData<ME.ECS.Camera.Camera>();
            Matrix4x4 projectionMatrix;
            if (camera.perspective == true) {
                
                projectionMatrix = Matrix4x4.Perspective(camera.fieldOfView, camera.aspect, camera.nearClipPlane, camera.farClipPlane);
                
            } else {

                projectionMatrix = Matrix4x4.Ortho(-camera.orthoSize, camera.orthoSize, -camera.orthoSize, camera.orthoSize, camera.nearClipPlane, camera.farClipPlane);
                
            }
            
            var worldToCameraMatrix = Matrix4x4.TRS(entity.GetPosition(), (Quaternion)entity.GetRotation(), Vector3.one);
            
            var screenSize = new Vector2(Screen.width, Screen.height);
            position.x *= screenSize.x;
            position.y *= screenSize.y;
            
            Matrix4x4 world2Screen = projectionMatrix * worldToCameraMatrix;
            Matrix4x4 screen2World = world2Screen.inverse;
            Vector3 screenSpace = world2Screen.MultiplyPoint(position);
            Vector3 worldSpace = screen2World.MultiplyPoint(screenSpace);

            return worldSpace;

        }

        /// <summary>
        /// UnityEngine.Camera::ViewportToScreenPoint
        /// </summary>
        /// <param name="entity">Entity with ME.ECS.Camera.Camera component</param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static UnityEngine.Vector3 ViewportToScreenPoint(this Entity entity, UnityEngine.Vector3 position) {

            if (entity.HasData<ME.ECS.Camera.Camera>() == false) return Vector3.zero;
            
            var camera = entity.GetData<ME.ECS.Camera.Camera>();
            Matrix4x4 projectionMatrix;
            if (camera.perspective == true) {
                
                projectionMatrix = Matrix4x4.Perspective(camera.fieldOfView, camera.aspect, camera.nearClipPlane, camera.farClipPlane);
                
            } else {

                projectionMatrix = Matrix4x4.Ortho(-camera.orthoSize, camera.orthoSize, -camera.orthoSize, camera.orthoSize, camera.nearClipPlane, camera.farClipPlane);
                
            }
            
            var worldToCameraMatrix = Matrix4x4.TRS(entity.GetPosition(), entity.GetRotation(), Vector3.one);
            
            var screenSize = new Vector2(Screen.width, Screen.height);
            position.x *= screenSize.x;
            position.y *= screenSize.y;
            
            Matrix4x4 world2Screen = projectionMatrix * worldToCameraMatrix;
            //Matrix4x4 screen2World = world2Screen.inverse;
            Vector3 screenSpace = world2Screen.MultiplyPoint(position);
            //Vector3 worldSpace = screen2World.MultiplyPoint(screenSpace);

            return screenSpace;

        }

        /// <summary>
        /// UnityEngine.Camera::ScreenToWorldPoint
        /// </summary>
        /// <param name="entity">Entity with ME.ECS.Camera.Camera component</param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static UnityEngine.Vector3 ScreenToWorldPoint(this Entity entity, UnityEngine.Vector3 position) {

            if (entity.HasData<ME.ECS.Camera.Camera>() == false) return Vector3.zero;
            
            var camera = entity.GetData<ME.ECS.Camera.Camera>();
            Matrix4x4 projectionMatrix;
            if (camera.perspective == true) {
                
                projectionMatrix = Matrix4x4.Perspective(camera.fieldOfView, camera.aspect, camera.nearClipPlane, camera.farClipPlane);
                
            } else {

                projectionMatrix = Matrix4x4.Ortho(-camera.orthoSize, camera.orthoSize, -camera.orthoSize, camera.orthoSize, camera.nearClipPlane, camera.farClipPlane);
                
            }
            
            var worldToCameraMatrix = Matrix4x4.TRS(entity.GetPosition(), entity.GetRotation(), Vector3.one);
            
            Matrix4x4 world2Screen = projectionMatrix * worldToCameraMatrix;
            Matrix4x4 screen2World = world2Screen.inverse;
            Vector3 screenSpace = world2Screen.MultiplyPoint(position);
            Vector3 worldSpace = screen2World.MultiplyPoint(screenSpace);

            return worldSpace;

        }

        /// <summary>
        /// UnityEngine.Camera::WorldToViewportPoint
        /// </summary>
        /// <param name="entity">Entity with ME.ECS.Camera.Camera component</param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static UnityEngine.Vector3 WorldToViewportPoint(this Entity entity, UnityEngine.Vector3 position) {

            if (entity.HasData<ME.ECS.Camera.Camera>() == false) return Vector3.zero;
            
            var camera = entity.GetData<ME.ECS.Camera.Camera>();
            Matrix4x4 projectionMatrix;
            if (camera.perspective == true) {
                
                projectionMatrix = Matrix4x4.Perspective(camera.fieldOfView, camera.aspect, camera.nearClipPlane, camera.farClipPlane);
                
            } else {

                projectionMatrix = Matrix4x4.Ortho(-camera.orthoSize, camera.orthoSize, -camera.orthoSize, camera.orthoSize, camera.nearClipPlane, camera.farClipPlane);
                
            }
            
            var worldToCameraMatrix = Matrix4x4.TRS(entity.GetPosition(), entity.GetRotation(), Vector3.one);
            
            var screenSize = new Vector2(Screen.width, Screen.height);
            Matrix4x4 world2Screen = projectionMatrix * worldToCameraMatrix;
            //Matrix4x4 screen2World = world2Screen.inverse;
            Vector3 screenSpace = world2Screen.MultiplyPoint(position);
            //Vector3 worldSpace = screen2World.MultiplyPoint(screenSpace);

            return new Vector3(screenSpace.x / screenSize.x, screenSpace.y / screenSize.y, screenSpace.z);

        }

        /// <summary>
        /// UnityEngine.Camera::WorldToScreenPoint
        /// </summary>
        /// <param name="entity">Entity with ME.ECS.Camera.Camera component</param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static UnityEngine.Vector3 WorldToScreenPoint(this Entity entity, UnityEngine.Vector3 position) {

            if (entity.HasData<ME.ECS.Camera.Camera>() == false) return Vector3.zero;

            var camera = entity.GetData<ME.ECS.Camera.Camera>();
            Matrix4x4 projectionMatrix;
            if (camera.perspective == true) {
                
                projectionMatrix = Matrix4x4.Perspective(camera.fieldOfView, camera.aspect, camera.nearClipPlane, camera.farClipPlane);
                
            } else {

                projectionMatrix = Matrix4x4.Ortho(-camera.orthoSize, camera.orthoSize, -camera.orthoSize, camera.orthoSize, camera.nearClipPlane, camera.farClipPlane);
                
            }
            
            var worldToCameraMatrix = Matrix4x4.TRS(entity.GetPosition(), entity.GetRotation(), Vector3.one);
            
            Matrix4x4 world2Screen = projectionMatrix * worldToCameraMatrix;
            //Matrix4x4 screen2World = world2Screen.inverse;
            Vector3 screenSpace = world2Screen.MultiplyPoint(position);
            //Vector3 worldSpace = screen2World.MultiplyPoint(screenSpace);

            return screenSpace;

        }

    }
}
