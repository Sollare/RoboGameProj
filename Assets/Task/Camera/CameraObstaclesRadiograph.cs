using UnityEngine;
using System.Collections;

public class CameraObstaclesRadiograph : MonoBehaviour {

	private Transform _transform;

	protected Transform cachedTransform
	{
		get
		{
			if (_transform) return transform;
			else return (_transform = transform);
		}
	}

	public Transform target;

	[Range(0,1)]
	public float opacity;
	public float dissolutionTime;

	public LayerMask obstacleLayer;

	private Transform currentObstacle;
	private Transform lastObstacle;
	//Препятствие в стадии появления
	private Transform nascentObstacle;

	private Shader lastShader;

	public Material TransparentMaterial;
	private Shader transparentShader;

	
	

	void Awake () {
		transparentShader = TransparentMaterial.shader;

	}

	void Update () {

		currentObstacle = obstacleHidesTarget(target);

		if(currentObstacle)
		{
			//Если цели не видно и препятствие не было запомнено
			if(lastObstacle == null)
			{
				//Делаем текущее препятствие прозрачным и запоминаем его.
				makeObstacleTransparent(currentObstacle);
				lastObstacle = currentObstacle;
			} 
			//Если цели не видно и до этого было запомнено препятствие, отличающееся от текущего
			if(lastObstacle != null && !currentObstacle.Equals(lastObstacle))
			{	
				//Делаем видимым запомненное препятствие, 
				showObstacle(lastObstacle);
				//Делаем прозрачным текущее препятствие и запоминаем его
				makeObstacleTransparent(currentObstacle);
				lastObstacle = currentObstacle;
			}
		}
		else
		{	
			//Если цель видно и есть запомненное препятствие
			if(lastObstacle != null)
			{	
				//Делаем его видимым и забываем
				showObstacle(lastObstacle);
				lastObstacle = null;
			}
		}
	}

	Transform obstacleHidesTarget(Transform playerTarget)
	{
		Ray ray;
		RaycastHit hit;
		ray = new Ray(cachedTransform.position, target.position - cachedTransform.position);
		
		if( (Physics.Raycast(ray,	out hit, Vector3.Distance(cachedTransform.position, target.position) * 0.9f, obstacleLayer)))
		{
			return hit.transform;
		} 
		else 
		{
			return null;
		}
	}

	void makeObstacleTransparent(Transform obstacle)
	{
		lastShader = obstacle.GetComponent<MeshRenderer>().material.shader;
		//Color clr = obstacle.renderer.material.color;
		//clr.a = opacity;
		obstacle.GetComponent<MeshRenderer>().material.shader = transparentShader;
		iTween.ColorTo(obstacle.gameObject, iTween.Hash("a",opacity,"time",dissolutionTime));
		//obstacle.renderer.material.color = clr;
	}

	void showObstacle(Transform obstacle)
	{	
		nascentObstacle = obstacle;
		iTween.ColorTo(obstacle.gameObject, iTween.Hash("a",1f,"time",dissolutionTime, "onComplete", "onNascentComplete", "onCompleteTarget", gameObject));
	}

	void onNascentComplete()
	{
		if(lastShader != null && nascentObstacle)
		{
			nascentObstacle.GetComponent<MeshRenderer>().material.shader = lastShader;
		}
		nascentObstacle = null;
	}

}

